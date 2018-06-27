using System.Collections;
using System.Collections.Generic;

using Google.Protobuf;

using UnityEngine;

namespace Game {
class StorageManager {
    public Ch.Config Config {
        get; private set;
    }
    public Ch.GameData GameData {
        get; private set;
    }

    void Awake() {
        var locale = PlayerPrefs.GetString("locale", "");
        if (string.IsNullOrEmpty(locale)) {
            InitStorage();
        }
        Config = Ch.Config.Parser.ParseFrom(
            System.Text.Encoding.UTF8.GetBytes(PlayerPrefs.GetString("config"))
        );
        GameData = Ch.GameData.Parser.ParseFrom(
            System.Text.Encoding.UTF8.GetBytes(PlayerPrefs.GetString("gamedata"))
        );
    }

    void InitStorage() {
        PlayerPrefs.SetString("locale", Application.systemLanguage.ToString());
        Ch.Config config = new Ch.Config();
        PlayerPrefs.SetString("config", 
            System.Text.Encoding.UTF8.GetString(config.Encode())
        );
        Ch.GameData gamedata = new Ch.GameData();
        for (int i = 0; i < Consts.PLAYER_DECK_NUM; i++) {
            gamedata.Decks.Add(new Ch.GameData.Types.Deck());
        }
        PlayerPrefs.SetString("gamedata", 
            System.Text.Encoding.UTF8.GetString(gamedata.Encode())
        );

        PlayerPrefs.Save();
    }
}
}