using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using System.IO;
using System.Text;
using static StorageManager;

public class PlayerDatas
{
    [JsonProperty("items")]
    public string[] ItemName { get; set; }
    [JsonProperty("amount")]
    public int[] Amount { get; set; }
    [JsonProperty("hp")]
    public int Hp { get; set; }
    [JsonProperty("hunger")]
    public float Hunger { get; set; }
    //[JsonProperty("location")]
    //public Vector2 Location { get; set; }
}

public class DataManager : MonoBehaviour
{
    public static PlayerDatas datas = new PlayerDatas();
    public PlayerStat stat = new PlayerStat();

    string path = @"userdata.json";

    string[] items = new string[28];
    int[] amount = new int[28];

    Dictionary<string, ItemSO> dic = new();

    public void Init()
    {
        Load();
    }

    public void Save()
    {
        var hotBarSlot = Managers.Inven.hotBarUI.slotList;
        var invenSlot = Managers.Inven.inventoryUI.slotList;

        for (int i = 0; i < items.Length; i++)
        {
            if (i <= 23)
            {
                if (invenSlot[i].itemUI.slotInfo.itemInfo != null)
                {
                    items[i] = invenSlot[i].itemUI.slotInfo.itemInfo.idName;
                    amount[i] = invenSlot[i].itemUI.slotInfo.count;
                }
            }
            else
            {
                if (hotBarSlot[i - 24].itemUI.slotInfo.itemInfo != null)
                {
                    items[i] = hotBarSlot[i - 24].itemUI.slotInfo.itemInfo.idName;
                    amount[i] = hotBarSlot[i - 24].itemUI.slotInfo.count;
                }
            }
        }
        datas = new PlayerDatas()
        {
            ItemName = items,
            Amount = amount,
            Hp = stat.Hp,
            Hunger = stat.Hunger
        };

        var json = JsonConvert.SerializeObject(datas, Formatting.None, new JsonSerializerSettings()
        {
            ReferenceLoopHandling = ReferenceLoopHandling.Ignore
        });


        Debug.Log(json);

        if (!File.Exists(path))
        {
            using (var file = File.Create("userdata.json"))
            {
                file.Write(Encoding.UTF8.GetBytes(json));
            }
        }
        else
        {
            var file = new FileStream(path, FileMode.Open);
            file.Write(Encoding.UTF8.GetBytes(json));
            file.Close();
        }

        Debug.Log("Complete to Save");
    }

    public void Load()
    {
        var items = Resources.LoadAll<ItemSO>("ItemSO/Item");
        foreach (var item in items)
        {
            dic.Add(item.idName, item);
        }

        if (!File.Exists(path))
        {
            Debug.Log("userdata.json is not found.");
        }
        else
        {
            var file = File.Open(path, FileMode.Open);
            byte[] buffer = new byte[1024];
            file.Read(buffer);
            datas = JsonConvert.DeserializeObject<PlayerDatas>(Encoding.UTF8.GetString(buffer));

            Set();

            file.Close();
        }
    }

    void Set()
    {
        if (datas == null)
        {
            Debug.Log("datas are empty");
            return;
        }

        //stat.Hp = datas.Hp;
        //stat.Hunger = datas.Hunger;

        var hotBarSlot = Managers.Inven.hotBarUI.slotList;
        var invenSlot = Managers.Inven.inventoryUI.slotList;
        for (int i = 0; i < items.Length; i++)
        {
            ItemSO data;
            if (datas.ItemName[i] != null && dic.TryGetValue(datas.ItemName[i], out data))
            {
                if (i <= 23) // ÀÎº¥
                {
                    invenSlot[i].itemUI.slotInfo.itemInfo = data;
                    invenSlot[i].itemUI.slotInfo.itemInfo.itemIcon = data.itemIcon;
                    invenSlot[i].itemUI.slotInfo.count = datas.Amount[i];
                }
                else // ÇÖ¹Ù
                {
                    hotBarSlot[i - 24].itemUI.slotInfo.itemInfo = data;
                    hotBarSlot[i - 24].itemUI.slotInfo.itemInfo.itemIcon = data.itemIcon;
                    hotBarSlot[i - 24].itemUI.slotInfo.count = datas.Amount[i];
                }
            }
        }

        Debug.Log("datas inputed");
    }
}
