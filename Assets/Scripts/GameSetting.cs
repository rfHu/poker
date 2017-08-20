using UniRx;

public static class GameSetting {
	// 设置
	private static string tagStr = "persist.txt?tag=";

    private static void setValue<T>(T value, string tag)
    {
        ES2.Save(value, tagStr + tag);
    }

    public static T getValue<T>(string tag) 
    {
        tag = tagStr + tag;
        if (ES2.Exists(tag)) 
        {
            return ES2.Load<T>(tag);
        }
        return default(T);
    }

    // 语音设置
    public static bool talkSoundClose {
        get {
			return getValue<bool>(PersistKey.TalkSound);
        }

        set {
            setValue(value, PersistKey.TalkSound);
        }
    }

    //游戏声音
	public static bool muted {
		get {
			return getValue<bool>(PersistKey.Mute);	
		}

		set {
			setValue(value, PersistKey.Mute);
		}
	}

    // 文字气泡
    public static bool chatBubbleClose {
        get
        {
			return getValue<bool>(PersistKey.ChatBubble);
        }

        set
        {
            setValue(value, PersistKey.ChatBubble);
        }
    }

    //动态表情
    public static bool emoticonClose{
        get
        {
			return getValue<bool>(PersistKey.EmoticonClose);
        }

        set
        {
            setValue(value, PersistKey.EmoticonClose);
        }
    }

	private static ReactiveProperty<int> cardColor;

    //卡牌颜色
    public static ReactiveProperty<int> CardColor {
		get {
			if (cardColor == null) {
				cardColor = new ReactiveProperty<int>(getValue<int>(PersistKey.CardColor));
				cardColor.Subscribe((value) => {
					setValue(value, PersistKey.CardColor);
				});
			}

			return cardColor;
		}
	}

	private static ReactiveProperty<int> tableSprite;

    //背景画面
    public static ReactiveProperty<int> TableSprite {
		get {
			if (tableSprite == null) {
				tableSprite = new ReactiveProperty<int>(getValue<int>(PersistKey.TableSprite));
				tableSprite.Subscribe((value) => {
					setValue(value, PersistKey.TableSprite);
				});
			}

			return tableSprite;
		}
	} 

	public static class PersistKey {
		public readonly static string CardColor = "cardColor";
		public readonly static string TableSprite = "tableSprite";
		public readonly static string TalkSound = "talkSound";
		public readonly static string Mute = "mute";
		public readonly static string ChatBubble = "chatBubble";
		public readonly static string EmoticonClose = "emoticonClose";
	}
}