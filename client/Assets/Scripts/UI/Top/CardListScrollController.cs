using System.Collections.Generic;
using System.Numerics;

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Game.UI {
[RequireComponent(typeof(InfiniteScroll))]
public class CardListScrollController : UIBehaviour, IInfiniteScrollSetup {

	public int currentMax_ = 1000; //hack to activate list elements 1st time.
	public InfiniteScroll infiniteScroll {
		get { 
			if (infiniteScroll_ == null) {
				infiniteScroll_ = GetComponent<InfiniteScroll>();
			}
			return infiniteScroll_;
		}
	}
	private InfiniteScroll infiniteScroll_;

	[HideInInspector]
	public List<KeyValuePair<BigInteger, Ch.Card>> Cards {
		get; set;
	}
	
	const int CELL_PER_ELEMENT = 3;

	public void OnPostSetupItems()
	{
		infiniteScroll.onUpdateItem.AddListener(OnUpdateItem);
		GetComponentInParent<ScrollRect>().movementType = ScrollRect.MovementType.Elastic;

		UpdateCurrentMax();
	}

	public void UpdateCurrentMax() {
		currentMax_ = Cards.Count;
		var limit = (int)((currentMax_ + CELL_PER_ELEMENT - 1) / CELL_PER_ELEMENT);

		var rectTransform = GetComponent<RectTransform>();
		var delta = rectTransform.sizeDelta;
		delta.y = infiniteScroll.itemScale * limit;
		rectTransform.sizeDelta = delta;		
	}

	public void UpdateCardList(List<KeyValuePair<BigInteger, Ch.Card>> list) {
		Cards = list;
		UpdateCurrentMax();
		infiniteScroll.RefreshList();
	}

	public void OnUpdateItem(int itemCount, GameObject obj)
	{
		var limit = (int)((currentMax_ + CELL_PER_ELEMENT - 1) / CELL_PER_ELEMENT);
		if(itemCount < 0 || itemCount >= limit) {
			obj.SetActive (false);
		}
		else {
			obj.SetActive (true);

			for (int i = 0; i < CELL_PER_ELEMENT; i++) {
				var idx = i + itemCount * CELL_PER_ELEMENT;
				if (idx >= Cards.Count) {
					obj.transform.Find("Name" + i).gameObject.SetActive(false);
					obj.transform.Find("Image" + i).gameObject.SetActive(false);
					continue;
				}
				var kv = Cards[idx];
				var name = obj.transform.Find("Name" + i).GetComponent<Text>();
				var img = obj.transform.Find("Image" + i).GetComponent<Image>();
				obj.transform.Find("Name" + i).gameObject.SetActive(true);
				obj.transform.Find("Image" + i).gameObject.SetActive(true);
				img.sprite = UIMgr.instance.catImages_[(int)kv.Key];
			}
		}
	}
}
}