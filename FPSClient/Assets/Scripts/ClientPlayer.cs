using DarkRift;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;
using TMPro;
using UnityEngine.UI;

struct ReconciliationInfo
{
    public ReconciliationInfo(uint frame, PlayerUpdateData data, PlayerInputData input)
    {
        Frame = frame;
        Data = data;
        Input = input;
    }

    public uint Frame;
    public PlayerUpdateData Data;
    public PlayerInputData Input;
}

public class ClientPlayer : MonoBehaviour
{
    [Header("Public Fields")]
    public ushort Id;
    public string Name;
    public bool IsOwn;
    public int Health;

    [Header("Variables")]
    public float SensitivityX;
    public float SensitivityY;

    [Header("References")]
    public PlayerLogic Logic;
    public PlayerInterpolation Interpolation;
    public TextMeshProUGUI NameText;
    public Image HealthBarFill;
    public GameObject HealthBarObject;

    [Header("Prefabs")]
    public GameObject ShotPrefab;

    private float yaw;
    private float pitch;

    private Queue<ReconciliationInfo> reconciliationHistory = new Queue<ReconciliationInfo>();

    // Start is called before the first frame update
    public void Initialize(ushort id, string name)
    {
        Id = id;
        Name = name;

        NameText.text = Name;
        SetHealth(100);

        if (GlobalManager.Instance.PlayerId == id)
        {
            IsOwn = true;
            Camera.main.transform.SetParent(transform);
            Camera.main.transform.localPosition = new Vector3(0, 0, -10);
            Camera.main.transform.localRotation = Quaternion.identity;
            Logic.CharacterController.enabled = false;
            transform.position = new Vector3(0, 50, 0);
            Interpolation.CurrentData = new PlayerUpdateData(Id, 0, new Vector3(5,0,12), Quaternion.identity);
            Logic.CharacterController.enabled = true;
        }

        SpawnStateInfo ssi = new SpawnStateInfo(new Vector3(5, 0, 12));

        using (Message m = Message.Create((ushort)Tags.SpawnDataInfo, ssi))
        {
            GlobalManager.Instance.Client.SendMessage(m, SendMode.Reliable);
        }

    }

    public void OnServerDataUpdate(PlayerUpdateData data)
    {
        if (IsOwn)
        {
            while (reconciliationHistory.Any() && reconciliationHistory.Peek().Frame < GameManager.Instance.LastRecievedServerTick)
            {
                reconciliationHistory.Dequeue();
            }

            if (reconciliationHistory.Any() && reconciliationHistory.Peek().Frame == GameManager.Instance.LastRecievedServerTick)
            {
                ReconciliationInfo info = reconciliationHistory.Dequeue();
                if (Vector3.Distance(info.Data.Position, data.Position) > 0.05f)
                {

                    List<ReconciliationInfo> infos = reconciliationHistory.ToList();
                    Interpolation.CurrentData = data;
                    transform.position = data.Position;
                    transform.rotation = data.LookDirection;
                    for (int i = 0; i < infos.Count; i++)
                    {
                        PlayerUpdateData u = Logic.GetNextFrameData(infos[i].Input, Interpolation.CurrentData);
                        Interpolation.SetFramePosition(u);
                    }
                }
            }
        }
        else
        {
            Interpolation.SetFramePosition(data);
        }
    }

    public void SetHealth(int value)
    {
        Health = value;
        HealthBarFill.fillAmount = value / 100f;
    }

    void LateUpdate()
    {
        Vector3 point = Camera.main.WorldToScreenPoint(transform.position + new Vector3(0, 1, 0));
        if (point.z > 2)
        {
            HealthBarObject.transform.position = point;
        }
        else
        {
            HealthBarObject.transform.position = new Vector3(10000, 0, 0);
        }
    }

    void FixedUpdate()
    {
        if (IsOwn)
        {
            bool[] inputs = new bool[6];
            inputs[0] = Input.GetKey(KeyCode.W);
            inputs[1] = Input.GetKey(KeyCode.A);
            inputs[2] = Input.GetKey(KeyCode.S);
            inputs[3] = Input.GetKey(KeyCode.D);
            inputs[4] = Input.GetKey(KeyCode.Space);
            inputs[5] = Input.GetMouseButton(0);

            if (inputs[5])
            {
                GameObject go = Instantiate(ShotPrefab);
                go.transform.position = Interpolation.CurrentData.Position;
                go.transform.rotation = transform.rotation;
                Destroy(go, 1f);
            }

            yaw += Input.GetAxis("Mouse X") * SensitivityX;
            pitch += Input.GetAxis("Mouse Y") * SensitivityY;

            Quaternion rot = Quaternion.Euler(pitch, yaw, 0);

            PlayerInputData inputData = new PlayerInputData(inputs, rot, 0);// GameManager.Instance.ClientTick);// 0/*here we later write the last recieved tick*/);

            //PlayerUpdateData updateData = Logic.GetNextFrameData(inputData, data);
            //transform.rotation = data.LookDirection;
            Debug.Log("position on update: " + Interpolation.CurrentData.Position);
            Logic.CharacterController.enabled = false;
            transform.position = Interpolation.CurrentData.Position;
            Logic.CharacterController.enabled = true;
            Debug.Log("position before: " + transform.position  + "and : " + Logic.gameObject.transform.position);
            PlayerUpdateData updateData = Logic.GetNextFrameData(inputData, Interpolation.CurrentData);
            Debug.Log("position after: " + transform.position);
            Interpolation.SetFramePosition(updateData);

            using (Message m = Message.Create((ushort)Tags.GamePlayerInput, inputData))
            {
                GlobalManager.Instance.Client.SendMessage(m, SendMode.Reliable);
            }

            reconciliationHistory.Enqueue(new ReconciliationInfo(GameManager.Instance.ClientTick, updateData, inputData));
        }
    }
}
