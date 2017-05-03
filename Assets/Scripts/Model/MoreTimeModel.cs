public class MoreTimeModel
{
    public int type;
    public int time;
    public int total;
    public string uid;

    public bool IsRound() {
        return type == 112;
    }
}