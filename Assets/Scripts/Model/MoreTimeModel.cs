public class MoreTimeModel
{
    public int type;
    public int time;
    public string uid;

    public bool IsRound() {
        return type == 112;
    }
}