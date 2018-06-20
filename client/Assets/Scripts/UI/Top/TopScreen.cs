using System.Numerics;
using System.Collections;
using System.Collections.Generic;

using UnityEngine;
using uGUI = UnityEngine.UI;

namespace Game.UI {
class TopScreen : MonoBehaviour {
    public enum Menu {
        InventoryMenu,
        MarketMenu,
        BreedMenu,
    }
    public uGUI.Text balance_;
    public uGUI.Button[] buttons_;
    public CardListScrollController scroll_;

    void Start() {
        balance_ = transform.Find("Balance").gameObject.GetComponent<uGUI.Text>();
        buttons_[(int)Menu.InventoryMenu].GetComponent<uGUI.Button>().onClick.AddListener(OnInventoryMenu);
        buttons_[(int)Menu.MarketMenu].GetComponent<uGUI.Button>().onClick.AddListener(OnMarketMenu);
        buttons_[(int)Menu.BreedMenu].GetComponent<uGUI.Button>().onClick.AddListener(OnBreedMenu);
        UpdateView();
        scroll_.Cards = ViewModel.ViewModelMgr.instance.Inventory.Cards;
    }

    public void UpdateView() {
        balance_.text = ViewModel.ViewModelMgr.instance.TokenBalance.ToString() + " DBC";
    }

    void OnInventoryMenu() {
        Debug.Log("OnInventory");
        scroll_.UpdateCardList(ViewModel.ViewModelMgr.instance.Inventory.Cards);
    }
    void OnMarketMenu() {
        Debug.Log("Market");
        //TODO: retrieve list of cats on sale 
        scroll_.UpdateCardList(new List<KeyValuePair<BigInteger, Ch.Card>>());        
    }
    void OnBreedMenu() {
        Debug.Log("Breed");        
        //TODO: retrieve list of cats which can breed 
        scroll_.UpdateCardList(new List<KeyValuePair<BigInteger, Ch.Card>>());
    }
}
}
