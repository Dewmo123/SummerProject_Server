namespace SummerGameServer.Services
{
    public static class Leveling
    {
        //단순히 100L씩 증가 나중에 기획하면 바꿔주기
        public static long RequiredExp(int level) => 100L * level;
    }

}
