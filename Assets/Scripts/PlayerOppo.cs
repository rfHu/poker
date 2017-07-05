using UnityEngine;
using UnityEngine.UI;
using UniRx;
using System.Collections.Generic;
using DG.Tweening;
using System.Collections;
using UnityEngine.UI.ProceduralImage;

namespace PokerPlayer {
    public class PlayerOppo: MonoBehaviour, PlayerDelegate {
        public PlayerBase Base;

	    public Text NameLabel;
        public Text CardDesc;
	    public List<Card> ShowCards;
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
            Countdown.gameObject.SetActive(false);                        
            Countdown.SetParent(Base.Circle, false);
            Countdown.SetAsFirstSibling();
        }

        public void Init(Player player, Transform parent) {
            Base.Init(player, parent.GetComponent<Seat>(), this);
            PlayerBase.SetInParent(transform, parent);

		    NameLabel.text = player.Name;

            if (player.InGame) {
				Cardfaces.gameObject.SetActive(true);
			}

            addEvents();
        }

        private void addEvents() {
            RxSubjects.ShowCard.Subscribe((e) => {
                var uid = e.Data.String("uid");
                if (uid != player.Uid) {
                    return ;
                }

                var cards = e.Data.IL("cards");
                showTheCards(cards, true);
            }).AddTo(this);

            // 中途复原行动
            player.Countdown.AsObservable().Where((obj) => obj.seconds > 0).Subscribe((obj) => {
                TurnTo(null, obj.seconds);
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

            Base.PlayerAct.gameObject.SetActive(false);		
        
            if (cards[0] > 0 || cards[1] > 0) {
                // 显示手牌
                if (cards[0] > 0) {
                    ShowCards[0].Show(cards[0], anim);
                } 

                if (cards[1] > 0) {
                    ShowCards[1].Show(cards[1], anim);
                }

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
            Cardfaces.DOMove(new Vector2(0, 200), duration).SetEase(ease);

            var image = Cardfaces.GetComponent<Image>();
            Tween tween; 

            if (image != null) {
                tween = image.DOFade(0.2f, duration).SetEase(ease);
            } else {
                var canvasGrp = Cardfaces.GetComponent<CanvasGroup>();
                tween = canvasGrp.DOFade(0.2f, duration).SetEase(ease);
            }

            tween.OnComplete(() => {
                Cardfaces.gameObject.SetActive(false);
            });
        }

        public void MoveOut() {
            Base.Avt.GetComponent<CircleMask>().Disable();
            activated = false;
        }

        public void TurnTo(Dictionary<string, object> data, int left) {
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
            Destroy(gameObject);
        }

        public void SeeCard(List<int> cards) {
            showTheCards(cards, player.SeeCardAnim);
        }
    }
}