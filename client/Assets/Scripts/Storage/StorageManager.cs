using System.Collections;
using System.Collections.Generic;
using System.Data;
using System.Linq;

using Mono.Data.Sqlite;
using Nethereum.Hex.HexConvertors.Extensions;
using Google.Protobuf;

using UnityEngine;

namespace Game {
public class StorageManager {
    const string SCHEMA_VERSION_TABLE = "record_versions";

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
        Config = Ch.Config.Parser.ParseFrom(PlayerPrefs.GetString("config").HexToByteArray());
        GameData = Ch.GameData.Parser.ParseFrom(PlayerPrefs.GetString("gamedata").HexToByteArray());

        LocalDbm = new SqliteConnection("Data Source=nch.db");
        LocalDbm.Open();
        CreateTable(SCHEMA_VERSION_TABLE, "name STRING NOT NULL PRIMARY KEY, gen INTEGER NOT NULL");
    }
    public Dictionary<byte[],R> LoadAllRecords<R>(
        string record_name, 
        Google.Protobuf.MessageParser<R> parser, 
        out int current_gen) 
        where R : Google.Protobuf.IMessage<R> {
        Dictionary<byte[],R> dict = new Dictionary<byte[],R>();
        try {
            CreateTable(record_name, "id BLOB NOT NULL PRIMARY KEY,blob BLOB NOT NULL");
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
                cmd.CommandText = "SELECT gen FROM " + SCHEMA_VERSION_TABLE + " WHERE name = @Name;";
                cmd.Parameters.Add(new SqliteParameter("Name", record_name));
                var tmp = cmd.ExecuteScalar();
                if (tmp != null) {
                    current_gen = (int)(long)tmp;
                } else {
                    using (var insert_cmd = LocalDbm.CreateCommand()) {
                        insert_cmd.CommandText = "INSERT INTO " + SCHEMA_VERSION_TABLE + " (name, gen) VALUES(@Name,0);";
                        insert_cmd.Parameters.Add(new SqliteParameter("Name", record_name));
                        insert_cmd.ExecuteNonQuery();
                    }
                    current_gen = 0;
                }
            }
        } catch(System.Exception exception) {
            current_gen = -1;
            Debug.LogError(exception.Message + "@" + exception.StackTrace);
        }        
        return dict;
    }
    public System.Exception SaveRecords<R>(int current_gen, string record_name, byte[][] ids, R[] records) 
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
            return null;
        } catch(System.Exception exception) {
            Debug.LogError(exception.Message + "@" + exception.StackTrace);
            return exception;
        }
    }

    void CreateTable(string record_name, string schema) {
        using (var tran = LocalDbm.BeginTransaction()) {
            using (var cmd = LocalDbm.CreateCommand()) {
                cmd.CommandText = "CREATE TABLE IF NOT EXISTS " + record_name + 
                    "(" + schema + ")";
                cmd.ExecuteNonQuery();
            }
            tran.Commit();
        }
    }

    void InitStorage() {
        PlayerPrefs.SetString("locale", Application.systemLanguage.ToString());
        Ch.Config config = new Ch.Config();
        PlayerPrefs.SetString("config", config.Encode().ToHex());
        Ch.GameData gamedata = new Ch.GameData();
        for (int i = 0; i < Consts.PLAYER_DECK_NUM; i++) {
            gamedata.Decks.Add(new Ch.GameData.Types.Deck());
        }
        PlayerPrefs.SetString("gamedata", gamedata.Encode().ToHex());
        PlayerPrefs.Save();
    }
}
}