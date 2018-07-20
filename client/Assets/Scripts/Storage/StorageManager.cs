using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;

using Mono.Data.Sqlite;

using Google.Protobuf;

using UnityEngine;

namespace Game {
public class StorageManager {
    public Ch.Config Config {
        get; private set;
    }
    public Ch.GameData GameData {
        get; private set;
    }
    protected System.Data.Common.DbConnection LocalDbm {
        get; private set;
    }


    public void Load() {
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

        LocalDbm = new SqliteConnection("Data Source=nch.db");
    }

    public Dictionary<byte[],R> LoadAllRecords<R>(
        string record_name, 
        Google.Protobuf.MessageParser<R> parser, 
        out int current_gen) 
        where R : Google.Protobuf.IMessage<R> {
        Dictionary<byte[],R> dict = new Dictionary<byte[],R>();
        using (var cmd = LocalDbm.CreateCommand()) {
            cmd.CommandText = "SELECT id,blob FROM " + record_name + ";";
            var r = cmd.ExecuteReader();
            byte[] kbs = new byte[256], vbs = new byte[256];
            while (r.Read()) {
                var krlen = r.GetBytes(0, 0, kbs, 0, kbs.Length);
                var vrlen = r.GetBytes(0, 0, vbs, 0, vbs.Length);
                dict[kbs.Take((int)krlen).ToArray()] = parser.ParseFrom(vbs.Take((int)vrlen).ToArray());
            }
        }
        using (var cmd = LocalDbm.CreateCommand()) {
            cmd.CommandText = "SELECT gen FROM record_versions WHERE name = @Name;";
            cmd.Parameters["Name"] = new SqliteParameter(System.Data.DbType.String, record_name);
            current_gen = (int)cmd.ExecuteScalar();
        }
        return dict;
    }
    public void SaveRecords<R>(int current_gen, string record_name, byte[][] ids, R[] records) 
        where R : Google.Protobuf.IMessage<R> {
        try {
            using (var tran = LocalDbm.BeginTransaction()) {
                using (var cmd = LocalDbm.CreateCommand()) {
                    cmd.CommandText = "INSERT INTO " + record_name + "(id,blob) VALUES (@Id,@Blob)";
                    for (int i = 0; i < ids.Length; i++) {
                        cmd.Parameters["Id"] = new SqliteParameter(System.Data.DbType.Binary, ids[i]);
                        cmd.Parameters["Blob"] = new SqliteParameter(System.Data.DbType.Binary, records[i].Encode());
                        cmd.ExecuteNonQuery();
                    }
                }
                using (var cmd = LocalDbm.CreateCommand()) {
                    cmd.CommandText = "UPDATE record_versions SET gen = @Gen WHERE name = @Name";
                    cmd.Parameters["Name"] = new SqliteParameter(System.Data.DbType.String, record_name);
                    cmd.Parameters["Gen"] = new SqliteParameter(System.Data.DbType.Int32, current_gen);
                    cmd.ExecuteNonQuery();
                }
                tran.Commit();
            }
        } catch(System.Exception exception) {
            Debug.LogError(exception.Message);
        }
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