using UnityEngine;
using UnityEngine.UI;
using TMPro;
public class RoomListObject : MonoBehaviour
{

    [Header("References")]
    public TextMeshProUGUI NameText;
    public TextMeshProUGUI SlotText;
    public Button JoinButton;

    public void Set(RoomData data)
    {
        NameText.text = data.Name;
        SlotText.text = data.Slots + "/" + data.MaxSlots;

        JoinButton.onClick.RemoveAllListeners();
        JoinButton.onClick.AddListener(delegate { LobbyManager.Instance.SendJoinRoomRequest(data.Name); });
    }
}