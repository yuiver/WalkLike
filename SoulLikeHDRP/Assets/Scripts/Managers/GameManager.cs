using Sirenix.Serialization;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using UnityEngine;
using Cinemachine;

[Serializable]
public class AESKey
{
    [OdinSerialize]
    public byte[] AES_Key;
    [OdinSerialize]
    public byte[] AES_Iv;
} 
public class GameData
{
    public int playerMaxHp = default;
    public int playerHp = default;
    public int playerMaxMana = default;
    public int playerMana = default;
    public int playerMaxStamina = default;
    public int playerStamina = default;
    public int playerAtk = default;
    public float playerSpeed = default;

    public int soul = default;

    public bool BGMOn = true;
    public bool SFXOn = true;
    public bool UI_SFXOn = true;
    public float[] _volumeValues = default;
}

public class GameManager : GSingleton<GameManager>
{
    public PlayerController playerController = default;
    public FreeLookController freeLookController = default;
    public CinemachineFreeLook freeLookCamera = default;

    public Vector3 stage1StartPosition = default;
    public Vector3 playerPosition = default;
    public Vector3 stage1_1MonsterPosition = default;
    public Vector3 stage1_2MonsterPosition = default;
    public Vector3 Stage1BossPosition = default;

    public int speed = 2;
    public bool isGameStop = false;

    GameData _gameData = new GameData();
    public GameData SaveData { get { return _gameData; } set { _gameData = value; } }

    //! 씬마다 쌓이는 메모리를 관리하기 위해 base.Clear를 호출하기 위한 함수
    public override void Clear()
    {
        //이 함수는 씬전환시 호출되야합니다.
        base.Clear();
    }

    #region Player

    public int playerMaxHp
    {
        get { return _gameData.playerMaxHp; }
        set { _gameData.playerMaxHp = value; }
    }
    public int playerHp
    {
        get { return _gameData.playerHp; }
        set { _gameData.playerHp = value; }
    }
    public int playerMaxMana
    {
        get { return _gameData.playerMaxMana; }
        set { _gameData.playerMaxMana = value; }
    }
    public int playerMana
    {
        get { return _gameData.playerMana; }
        set { _gameData.playerMana = value; }
    }
    public int playerStamina
    {
        get { return _gameData.playerStamina; }
        set { _gameData.playerStamina = value; }
    }    
    public int playerMaxStamina
    {
        get { return _gameData.playerMaxStamina; }
        set { _gameData.playerMaxStamina = value; }
    }
    public int playerAtk
    {
        get { return _gameData.playerAtk; }
        set { _gameData.playerAtk = value; }
    }

    public float playerSpeed
    {
        get {return _gameData.playerSpeed; }
        set { _gameData.playerSpeed = value; }
    }
    public int soul
    {
        get { return _gameData.soul; }
        set { _gameData.soul = value; }
    }
    #endregion

    #region Option

    public bool BGMOn
    {
        get { return _gameData.BGMOn; }
        set { _gameData.BGMOn = value; }
    }
    public bool SFXOn
    {
        get { return _gameData.SFXOn; }
        set { _gameData.SFXOn = value; }
    }
    public bool UI_SFXOn
    {
        get { return _gameData.UI_SFXOn; }
        set { _gameData.UI_SFXOn = value; }
    }
    public float[] _volumeValues
    {
        get { return _gameData._volumeValues; }
        set
        {
            for (int i = 0; i < value.Length; i++)
            {
                value[i] = Mathf.Clamp01(value[i]);
            }
            _gameData._volumeValues = value; 
        }
    }
#endregion


public bool IsLoaded = false;

    //아직 미완성이라 세이브 파일을 삭제하지 않으면 저장되지 않는다. 수치를 변경하고 저장하는 함수를 런타임에서 구현하는게 아니라면 init을 수정해도 아무것도 변하는게 없다.
    protected override void Init()
    {
        AES_SavePath = Application.persistentDataPath + "/UserKey.bytes";
        SavePath = Application.persistentDataPath + "/UserSave.bytes";
        if (LoadGame())
        {
            return;
        }
        playerMaxHp = 150;
        playerHp = playerMaxHp;
        playerMaxMana = 100;
        playerMana = playerMaxMana;
        playerMaxStamina = 300;
        playerStamina = playerMaxStamina;
        playerAtk = 30;
        //playerStamina 
        playerSpeed = 10;
        soul = 0;
        _volumeValues = new float[(int)Sound.MaxCount] { 1f, 1f, 1f };
        IsLoaded = true;
        isGameStop = false;
        SaveGame();
    }

    #region 소울을 위한 함수들
    public bool CheckSoul(int _soul)
    {
        if (soul >= _soul)
            return true;
        else
            return false;
    }
    public bool SpendSoul(int _soul)
    {
        if (CheckSoul(_soul))
        {
            soul -= _soul;
            //if (Managers.UI.SceneUI is UI_SelectStageScene)
            //{
            //    (Managers.UI.SceneUI as UI_SelectStageScene).TopUI.Refresh();
            //}
            return true;
        }

        return false;
    }
    public void GetSoul(int _soul)
    {
        soul += _soul;
        //if (Managers.UI.SceneUI is UI_SelectStageScene)
        //{
        //    (Managers.UI.SceneUI as UI_SelectStageScene).TopUI.Refresh();
        //}
    }
    #endregion

    public float GetVolume(Sound type)
    {
        return _volumeValues[(int)type];
    }


    #region Save&Load
    private string SavePath = default;

    public void SaveGame()
    {
        AESKey AESKey = CreatOrLoadAESkey();

        byte[] bytes = SerializationUtility.SerializeValue(SaveData, DataFormat.Binary);
        bytes = AESHelper.Encrypt(bytes, AESKey.AES_Key, AESKey.AES_Iv);
        System.Text.Encoding.Default.GetBytes(SavePath);
        File.WriteAllBytes(SavePath, bytes);
        GFunc.Log($"Game data saved to: {SavePath}");
    }

    public bool LoadGame()
    {
        AESKey AESKey = CreatOrLoadAESkey();

        if (File.Exists(SavePath) == false)
        {
            GFunc.LogWarning($"No save file found at: {SavePath}");
            return false;
        }
        byte[] bytes = File.ReadAllBytes(SavePath);
        bytes = AESHelper.Decrypt(bytes, AESKey.AES_Key, AESKey.AES_Iv);
        GameData data = SerializationUtility.DeserializeValue<GameData>(bytes, DataFormat.Binary);
        GFunc.Log($"Game data loaded to: {SavePath}");

        if (data != null)
        {
            SaveData = data;
        }
        IsLoaded = true;
        return true;
    }
    #endregion

    #region Save&Load AES256Key

    private string AES_SavePath = default;

    //! AES-256 키가 생성될때 기본 경로에 저장해주는 함수
    private void SaveAESData(AESKey data)
    {
        byte[] bytes = SerializationUtility.SerializeValue(data, DataFormat.Binary);
        System.Text.Encoding.Default.GetBytes(AES_SavePath);
        File.WriteAllBytes(AES_SavePath, bytes);
    }
    //! AES-256 키가 저장된 파일이 있다면 불러오고 없다면 새로 생성해서 저장해주는 함수
    private AESKey CreatOrLoadAESkey()
    {
        if (File.Exists(AES_SavePath))
        {
            byte[] bytes = File.ReadAllBytes(AES_SavePath);
            AESKey data = SerializationUtility.DeserializeValue<AESKey>(bytes, DataFormat.Binary);
            GFunc.Log($"Game data loaded to: {AES_SavePath}");
            return data;
        }
        else
        {
            GFunc.LogWarning($"No save file found at: {AES_SavePath}");

            byte[] makeKey = new byte[32];
            using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(makeKey);
            }

            // 128비트(16바이트) 길이의 초기화 벡터 생성
            byte[] makeIv = new byte[16];
            using (RNGCryptoServiceProvider rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(makeIv);
            }

            // 기본값을 가진 GameSaveData 객체 생성
            AESKey defaultData = new AESKey
            {
                AES_Key = makeKey,
                AES_Iv = makeIv
            };

            // 기본 데이터 저장
            SaveAESData(defaultData);

            // 기본 데이터 반환
            return defaultData;
        }
    }
    #endregion

}
