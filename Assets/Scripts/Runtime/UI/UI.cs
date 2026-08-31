using UnityEngine;

public class UI : MonoBehaviour
{
    [Header("필수 연결 목록")]
    [SerializeField] private Player _player;
    [SerializeField] private Inventory _inventory;
    [SerializeField] private WeaponManager _weaponManager;


    private void Awake()
    {
        Cursor.visible = false; // 마우스 커서 없애기
        Cursor.lockState = CursorLockMode.Locked; // 마우스 잠그기
    }
    private void OnGUI()
    {
        #region CrossHair
        // 크로스헤어
        float crossHairWidth = 30f;
        float crossHairHeight = 4f;
        float subPush = 8f;
        float recoilOffset = 10f;
        float curRecoil = _weaponManager.CurRecoil;

        Texture2D greenTexture;
        Color color = new Color(0, 1f, 0, 0.2f);
        greenTexture = new Texture2D(1, 1);
        greenTexture.SetPixel(0, 0, color);
        greenTexture.Apply();

        GUIStyle box = new GUIStyle(GUI.skin.box);
        box.normal.background = greenTexture;

        GUI.Box(new Rect((Screen.width - crossHairHeight) * 0.5f, (Screen.height - crossHairHeight) * 0.5f, crossHairHeight, crossHairHeight), "", box);

        GUI.Box(new Rect((Screen.width * 0.5f - crossHairWidth - subPush) - curRecoil * recoilOffset, (Screen.height - crossHairHeight) * 0.5f, crossHairWidth, crossHairHeight), "", box);
        GUI.Box(new Rect((Screen.width * 0.5f + subPush) + curRecoil * recoilOffset, (Screen.height - crossHairHeight) * 0.5f, crossHairWidth, crossHairHeight), "", box);

        GUI.Box(new Rect((Screen.width - crossHairHeight) * 0.5f, (Screen.height * 0.5f - crossHairWidth - subPush) - curRecoil * recoilOffset, crossHairHeight, crossHairWidth), "", box);
        GUI.Box(new Rect((Screen.width - crossHairHeight) * 0.5f, (Screen.height * 0.5f + subPush) + curRecoil * recoilOffset, crossHairHeight, crossHairWidth), "", box);
        #endregion
    }
}
