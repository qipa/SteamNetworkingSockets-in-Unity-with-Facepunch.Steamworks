using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Character : MonoBehaviour
{
    public byte id;
    public ulong steamId;
    public string username;

    public float walkingSpeed = 3;
    public float runningMultiplier = 1.65f;
    public float acceleration = 5;

    public NetworkTestPrefab ui;
    public CharacterController cont;
    public Transform lerpPos;
    public Transform camera;
    float yRot;

    

    float lerpTime = 0;
    List<Vector3> positions = new List<Vector3>();
    List<float> posTimes = new List<float>();

    private void Awake()
    {
        cont = GetComponent<CharacterController>();
        if (id == steamNetworkClient.myID)
        {
            setupBuffer();
        }
        posTimes.Add(0);
        posTimes.Add(0);
        positions.Add(Vector3.zero);
        positions.Add(Vector3.zero);
    }
    private void Update()
    {
        lerpTime += Time.deltaTime;
        float lT = posTimes[1] - posTimes[0];
        lerpPos.position = Vector3.Lerp(positions[0], positions[1], lerpTime / lT);

        if (id != steamNetworkClient.myID)
        {
            return;
        }
        transform.Rotate(0, Input.GetAxis("Mouse X"), 0);
        yRot -= Input.GetAxis("Mouse Y");
        yRot = Mathf.Clamp(yRot, -80, 80);
        camera.localEulerAngles = new Vector3(yRot, 0, 0);
    }
    private void FixedUpdate()
    {
        //local client stuff
        if (id == steamNetworkClient.myID)
        {
            bool[] _inputs = new bool[]
            {
                Input.GetKey(KeyCode.W),
                Input.GetKey(KeyCode.A),
                Input.GetKey(KeyCode.S),
                Input.GetKey(KeyCode.D),
                Input.GetKey(KeyCode.LeftShift)
            };
            ClientSend.sendInputToServer(tick, _inputs, transform.eulerAngles.y, yRot);
            inputs = _inputs;

            if (SteamManager.server == null)
            {
                if (id == steamNetworkClient.myID)
                {
                    uint slot = tick % 1024;
                    buffer[slot].position = transform.position;
                    buffer[slot].velocity = velocity;
                    currentInputs = new Inputs();
                    currentInputs.inputs = _inputs;
                    inputBuffer[slot] = currentInputs;
                    setInput(tick, _inputs);
                    move();
                }
                ++tick;
            }
            if (Steamworks.SteamClient.IsValid)
            {
                if (Steamworks.SteamUser.HasVoiceData)
                {
                    byte[] v = Steamworks.SteamUser.ReadVoiceDataBytes();
                    ClientSend.sendVoice(v);
                }
                Steamworks.SteamUser.VoiceRecord = Input.GetKey(KeyCode.V);
            }
        }

        //client stuff
        if (SteamManager.server == null)
        {

        }
        // server stuff
        if (SteamManager.server != null)
        {
            //Debug.Log(id);
            move();
        }
    }

    private struct State
    {
        public Vector3 position;
        public Vector3 velocity;
    }
    private struct Inputs
    {
        public bool[] inputs;
    }
    uint tick;
    State[] buffer = new State[1024];
    Inputs[] inputBuffer = new Inputs[1024];
    Vector2 inputDirection;
    Vector3 velocity = Vector3.zero;
    Inputs currentInputs;
    bool[] inputs;
    void setupBuffer()
    {
        for (int i = 0; i < buffer.Length; i++)
        {
            buffer[i] = new State();
        }
    }
    public void setInput(uint _tick, bool[] _inputs)
    {
        tick = _tick;
        inputs = _inputs;
        speedTarget = 0;

        inputDirection = Vector2.zero;
        if (inputs[0])
        {
            inputDirection.y += 1;
            speedTarget = walkingSpeed;
        }
        if (inputs[1])
        {
            inputDirection.x -= 1;
            speedTarget = walkingSpeed;
        }
        if (inputs[2])
        {
            inputDirection.y -= 1;
            speedTarget = walkingSpeed;
        }
        if (inputs[3])
        {
            inputDirection.x += 1;
            speedTarget = walkingSpeed;
        }
        if (inputs[4])
        {
            speedTarget *= runningMultiplier;
        }
    }

    Vector2 velocityXZ = Vector2.zero;
    float speedTarget;
    void move()
    {

        //MOVEMENT CODE STARTED

        if (cont.isGrounded)
        {
            velocity.y = 0;
        }

        //old code from old demo:
        //Vector3 inputDir = Vector3.Normalize(transform.right * inputDirection.x + transform.forward * inputDirection.y) * movementSpeed;
        //velocity.x = inputDir.x * Time.fixedDeltaTime;
        //velocity.z = inputDir.z * Time.fixedDeltaTime;
        //velocity.y += -9.81f * Time.fixedDeltaTime * Time.fixedDeltaTime;

        Vector2 forward = new Vector2(transform.forward.x, transform.forward.z);
        Vector2 right = new Vector2(transform.right.x, transform.right.z);
        Vector2 inputDir = Vector3.Normalize(right * inputDirection.x + forward * inputDirection.y);
        velocityXZ = Vector2.MoveTowards(velocityXZ, inputDir.normalized * speedTarget, Time.fixedDeltaTime * acceleration);
        //velocityXZ = Vector2.ClampMagnitude(velocityXZ, speedTarget);
        velocity.x = velocityXZ.x * Time.fixedDeltaTime;
        velocity.z = velocityXZ.y * Time.fixedDeltaTime;
        velocity.y += -9.81f * Time.fixedDeltaTime * Time.fixedDeltaTime;


        cont.enabled = true;
        cont.Move(velocity);
        cont.enabled = false;

        ///MOVEMENT CODE OVER

        if (id == steamNetworkClient.myID || SteamManager.server != null)
        {
            lerpTime = 0;
            positions.Add(transform.position);
            posTimes.Add(Time.time);
            positions.Remove(positions[0]);
            posTimes.Remove(posTimes[0]);
        }
        if (SteamManager.server != null)
        {
            tick++;
            ServerSend.sendPosition(id, transform.position, tick);
        }   
    }
    public void setRotation(float rotation, float _yRot)
    {
        if (id != steamNetworkClient.myID)
        {
            transform.localEulerAngles = new Vector3(0, rotation, 0);
            if (camera != null)
            {
                camera.localEulerAngles = new Vector3(_yRot, 0, 0);
            }
        }
        ServerSend.sendRotation(id, transform.eulerAngles.y, _yRot);
    }

    public void setPosition(uint _tick, Vector3 position)
    {
        if (id != steamNetworkClient.myID)
        {
            //Debug.Log(position);
            cont.enabled = false;
            lerpTime = 0;
            positions.Add(position);
            posTimes.Add(Time.time);
            positions.Remove(positions[0]);
            posTimes.Remove(posTimes[0]);
            transform.position = position;
            return;
        }
        uint bufferSlot = _tick % 1024;
        Vector3 error = position - buffer[bufferSlot].position;
        if (error.sqrMagnitude > .0000001f)
        {
            //Debug.Log(tick);
            //Debug.Log(_tick);
            cont.enabled = false;
            transform.position = position;
            cont.enabled = true;
            uint rewindTick = _tick;
            while(rewindTick < tick)
            {
                bufferSlot = rewindTick % 1024;
                velocity = buffer[bufferSlot].velocity;
                buffer[bufferSlot].position = transform.position;

                setInput(tick, inputBuffer[bufferSlot].inputs);
                move();

                ++rewindTick;
            }
        }
    }

    public AudioSource voiceChatSource;
    public float chatVol = 1;
    uint optimalRate;
    int voiceBufferSize;
    int dataPosition = 0;
    int playbackvoiceBuffer = 0;
    int dataReceived = 0;

    AudioClip c;
    float[] voiceBuffer;

    public void voiceChat(byte[] d)
    {
        if (c == null)
        {
            optimalRate = Steamworks.SteamUser.OptimalSampleRate;
            voiceBufferSize = (int)optimalRate * 5;
            voiceBuffer = new float[voiceBufferSize];
            c = createClip();
            voiceChatSource.clip = c;
            voiceChatSource.Play();
        }
        var output = new System.IO.MemoryStream();
        int dataSize = Steamworks.SteamUser.DecompressVoice(d, output);
        WriteToClip(ReadToEnd(output), dataSize);
    }
    AudioClip createClip()
    {
        return AudioClip.Create("voiceChat", (int)256, 1, (int)optimalRate, true, OnAudioRead, null);
    }

    private void OnAudioRead(float[] data)
    {
        for (int i = 0; i < data.Length; ++i)
        {
            data[i] = 0;

            if (playbackvoiceBuffer > 0)
            {
                dataPosition++;
                playbackvoiceBuffer -= 1;

                data[i] = voiceBuffer[dataPosition % voiceBufferSize];
                data[i] *= chatVol;
            }
        }
    }

    void WriteToClip(byte[] uncompressed, int iSize)
    {
        for (int i = 0; i < iSize; i += 2)
        {
            WriteToClip((short)(uncompressed[i] | uncompressed[i + 1] << 8) / 32767.0f);
        }
    }

    void WriteToClip(float f)
    {
        voiceBuffer[dataReceived % voiceBufferSize] = f;
        dataReceived++;
        playbackvoiceBuffer++;
    }
    public static byte[] ReadToEnd(System.IO.MemoryStream stream)
    {
        long originalPosition = 0;

        if (stream.CanSeek)
        {
            originalPosition = stream.Position;
            stream.Position = 0;
        }

        try
        {
            byte[] readvoiceBuffer = new byte[4096];

            int totalBytesRead = 0;
            int bytesRead;

            while ((bytesRead = stream.Read(readvoiceBuffer, totalBytesRead, readvoiceBuffer.Length - totalBytesRead)) > 0)
            {
                totalBytesRead += bytesRead;

                if (totalBytesRead == readvoiceBuffer.Length)
                {
                    int nextByte = stream.ReadByte();
                    if (nextByte != -1)
                    {
                        byte[] temp = new byte[readvoiceBuffer.Length * 2];
                        System.Buffer.BlockCopy(readvoiceBuffer, 0, temp, 0, readvoiceBuffer.Length);
                        System.Buffer.SetByte(temp, totalBytesRead, (byte)nextByte);
                        readvoiceBuffer = temp;
                        totalBytesRead++;
                    }
                }
            }

            byte[] voiceBuffer = readvoiceBuffer;
            if (readvoiceBuffer.Length != totalBytesRead)
            {
                voiceBuffer = new byte[totalBytesRead];
                System.Buffer.BlockCopy(readvoiceBuffer, 0, voiceBuffer, 0, totalBytesRead);
            }
            return voiceBuffer;
        }
        finally
        {
            if (stream.CanSeek)
            {
                stream.Position = originalPosition;
            }
        }
    }
}
