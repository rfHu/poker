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

		public List<Image> OmahaCards;

		[SerializeField]private GameObject cardParent;

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

        [SerializeField]
        private ParticleSystem countdownParticle;

        private GameObject particleParent {
            get {
                return countdownParticle.transform.parent.gameObject;
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

        void OnDespawned() {
            this.Dispose(); 

            CardDesc.transform.parent.gameObject.SetActive(false);
            Cardfaces.GetComponent<RectTransform>().anchoredPosition = new Vector2(40, -40);
            Cardfaces.GetComponent<CanvasGroup>().alpha = 1;

            MoveOut();
			hideCards();
            cardParent.SetActive(false);
            NameLabel.gameObject.SetActive(true);
			stopParticle(countdownParticle);
        }

		private void hideCards() {
			foreach(var c in cardContainers) {
				if (c.CardInstance != null) {
					c.CardInstance.Turnback();
				}
				c.gameObject.SetActive(false);
			}
		} 

		public static void Init(Player player, Seat seat) {
			var transform = PoolMan.Spawn("PlayerOppo", seat.transform);
			transform.GetComponent<RectTransform>().anchoredPosition = Vector3.zero;
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

			GameData.Shared.Type.Subscribe((type) => {
				foreach(var card in OmahaCards) {
					card.enabled = type == GameType.Omaha;
				}

				cardParent.GetComponent<HorizontalLayoutGroup>().spacing = type == GameType.Omaha ? -48 : 0;
			}).AddTo(this);
        }

        private void showCardType(int maxFive) {
            var desc = Card.GetCardDesc(maxFive);

            if (string.IsNullOrEmpty(desc)) {
                return ;
            }

            CardDesc.transform.parent.gameObject.SetActive(true);
            CardDesc.transform.parent.GetComponent<CanvasGroup>().alpha = 1;
            CardDesc.text = desc;
            NameLabel.gameObject.SetActive(false);
        }

        private void showTheCards(List<int> cards, bool anim) {
            if (cards.Count < 2) {
                return ;
            }
            cardParent.GetComponent<CanvasGroup>().alpha = 1;
            Base.PlayerAct.SetActive(false);		
        
            if (cards.Any(c => c > 0)) {
                cardParent.SetActive(true);

                // 显示手牌
      			showCards(cards, anim);

                if (Cardfaces != null) {
                    Cardfaces.gameObject.SetActive(false);
                }
            }
        }

		private void showCards(List<int> cards, bool anim) {
			for (var i = 0; i < cards.Count; i++) {
				var c = cardContainers[i];
				c.gameObject.SetActive(true);
				c.CardInstance.Show(cards[i], anim);
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

            playParticle(countdownParticle);

            while (time > 0 && activated) {
                time = time - Time.deltaTime;
                
                var percent = Mathf.Min(1, time / total);
                var image = Countdown.GetComponent<ProceduralImage>();
                image.fillAmount = percent;
                mask.SetTextColor(image.color);
                mask.SetFillAmount(time / total, time); 

                particleParent.transform.localRotation =Quaternion.Euler(new Vector3(0,0,360* percent));

                yield return new WaitForFixedUpdate();
            }

            activated = false;
            Countdown.gameObject.SetActive(false);
            stopParticle(countdownParticle);
        }

        // ============= Delegate ============
        public void Fold() {
            MoveOut();

            var duration = 0.5f;

            Ease ease = Ease.Flash;
            Cardfaces.DOMove(Controller.LogoVector, duration).SetEase(ease).SetId(Base.AnimID);

			var canvasGrp = Cardfaces.GetComponent<CanvasGroup>();
			var tween = canvasGrp.DOFade(0, duration).SetEase(ease).SetId(Base.AnimID);

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

        private void playParticle(ParticleSystem particle) {
            particle.gameObject.SetActive(true);
            particle.Play(true);
        }

        private void stopParticle(ParticleSystem particle) {
            particle.gameObject.SetActive(false);
            particle.Stop(true);
        }
    }
}