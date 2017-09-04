using UnityEngine;
using UnityEngine.UI;
using UniRx;
using System.Collections.Generic;
using DG.Tweening;
using System.Collections;
using UnityEngine.UI.ProceduralImage;
using System.Linq;

namespace PokerPlayer {
    public class PlayerOppo: MonoBehaviour, PlayerDelegate {
        public GameObject BaseObject;
		public PlayerBase Base;

	    public Text NameLabel;
        public Text CardDesc;
        public GameObject WinPercent;

		private GameObject cardParent {
			get {
				return cardContainers[0].transform.parent.gameObject;
			}
		}

		private Card card1 {
			get {
				return cardContainers[0].CardInstance;
			}
		}

		private Card card2 {
			get {
				return cardContainers[1].CardInstance;
			}
		}


        [SerializeField]
        private List<CardContainer> cardContainers; 

        public Transform Cardfaces;
        public Transform Countdown;

        private IEnumerator turnFactor;
        private bool activated;

        private Player player {
            get {
                return Base.player;
            }
        }

        void Awake()
        {
			Base = PlayerBase.Load(BaseObject, transform);

            Countdown.gameObject.SetActive(false);                        
            Countdown.SetParent(Base.Circle, false);
            Countdown.SetAsFirstSibling();

            Cardfaces.SetParent(Base.Circle, false);
            Cardfaces.SetSiblingIndex(3);

            cardParent.transform.SetParent(Base.Circle, false);
            cardParent.transform.SetAsLastSibling();
        }

        void OnDestroy()
	{
	}

        void OnDespawned() {
            this.Dispose(); 

            CardDesc.transform.parent.gameObject.SetActive(false);
            Cardfaces.GetComponent<RectTransform>().anchoredPosition = new Vector2(40, -20);
            Cardfaces.GetComponent<CanvasGroup>().alpha = 1;

            MoveOut();
			turnbackCards();
            cardParent.SetActive(false);
            NameLabel.gameObject.SetActive(true);
        }

		private void turnbackCards() {
			if (card1 != null) {
				card1.Turnback();
			}

			if (card2 != null) {
				card2.Turnback();
			}
		}

		public static void Init(Player player, Seat seat) {
			var transform = PoolMan.Spawn("PlayerOppo", seat.transform);
			transform.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, 0);
			transform.GetComponent<PlayerOppo>().init(player, seat);
		}

        private void init(Player player, Seat seat) {
            Base.Init(player, seat, this);

		    NameLabel.text = player.Name;

            if (player.InGame) {
				Cardfaces.gameObject.SetActive(true);
			} else {
                Cardfaces.gameObject.SetActive(false);
            }

            addEvents();
        }

        private void addEvents() {
            // 中途复原行动
            player.Countdown.AsObservable().Where((obj) => obj.seconds > 0).Subscribe((obj) => {
                TurnTo(null, obj.seconds);
            }).AddTo(this);

            player.WinPercent.AsObservable().Subscribe((num) =>
            {
                if (num == -1)
                {
                    WinPercent.GetComponent<ProceduralImage>().color = _.HexColor("#868d94");
                    WinPercent.SetActive(false);
                }
                WinPercent.SetActive(true);
                WinPercent.GetComponentInChildren<Text>().text = num + "%";
            }).AddTo(this);

            player.Largest.AsObservable().Subscribe((n) =>
            {
                WinPercent.GetComponent<ProceduralImage>().color = _.HexColor("#ff1744");
            }).AddTo(this);
        }

        private void showCardType(int maxFive) {
            var desc = Card.GetCardDesc(maxFive);

            if (string.IsNullOrEmpty(desc)) {
                return ;
            }

            CardDesc.transform.parent.gameObject.SetActive(true);
            CardDesc.text = desc;
            NameLabel.gameObject.SetActive(false);
        }

        private void showTheCards(List<int> cards, bool anim) {
            if (cards.Count < 2) {
                return ;
            }

            Base.PlayerAct.SetActive(false);		
        
            if (cards[0] > 0 || cards[1] > 0) {
                cardParent.SetActive(true);

                // 显示手牌
      			card1.Show(cards[0], anim);
                card2.Show(cards[1], anim);

                if (Cardfaces != null) {
                    Cardfaces.gameObject.SetActive(false);
                }
            }
        }

        private IEnumerator turnTo(int left) {
            Countdown.gameObject.SetActive(true);
            activated = true;

            Countdown.GetComponent<Animator>().SetTrigger("1");
            Countdown.GetComponent<Animator>().speed = 1 / (float)left;

            float time = (float)left;
            float total = time.GetThinkTime();
            var mask = Base.Avt.GetComponent<CircleMask>();
            mask.numberText.gameObject.SetActive(true);

            while (time > 0 && activated) {
                time = time - Time.deltaTime;
                
                var percent = Mathf.Min(1, time / total);
                var image = Countdown.GetComponent<ProceduralImage>();
                image.fillAmount = percent;
                mask.SetTextColor(image.color);
                mask.SetFillAmount(time / total, time); 

                yield return new WaitForFixedUpdate();
            }

            activated = false;
            Countdown.gameObject.SetActive(false); 
        }

        // ============= Delegate ============
        public void Fold() {
            MoveOut();

            var duration = 0.5f;

            Ease ease = Ease.Flash;
            Cardfaces.DOMove(Controller.LogoVector, duration).SetEase(ease).SetId(Base.AnimID);

            var image = Cardfaces.GetComponent<Image>();
            Tween tween; 

            if (image != null) {
                tween = image.DOFade(0.2f, duration).SetEase(ease).SetId(Base.AnimID);
            } else {
                var canvasGrp = Cardfaces.GetComponent<CanvasGroup>();
                tween = canvasGrp.DOFade(0.2f, duration).SetEase(ease).SetId(Base.AnimID);
            }

            tween.OnComplete(() => {
                Cardfaces.gameObject.SetActive(false);
            });
        }

        public void SetFolded() {

        }

        public void MoveOut() {
            Base.Avt.GetComponent<CircleMask>().Disable();
            activated = false;
            Countdown.gameObject.SetActive(false);
        }

        public void TurnTo(Dictionary<string, object> data, int left) {
			PlayerBase.CurrentUid = player.Uid;

            if (turnFactor != null) {
                StopCoroutine(turnFactor);
            }

            turnFactor = turnTo(left);
            StartCoroutine(turnFactor);
        }

        public void ResetTime(int total) {
             TurnTo(null, total);
        }

        public void Despawn() {
            PoolMan.Despawn(transform);
        }

        public void ShowCard(List<int> cards) {
            showTheCards(cards, player.SeeCardAnim);
        }

        public void HandOver(GameOverJson data) {
            showTheCards(data.cards, true);
            showCardType(data.maxFiveRank);
        }

        public void WinEnd() {
            Base.DoFade(cardParent);
            Base.DoFade(CardDesc.transform.parent.gameObject, () => {
                NameLabel.gameObject.SetActive(true);
            });
        }
    }
}