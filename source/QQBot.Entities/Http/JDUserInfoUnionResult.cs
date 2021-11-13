namespace QQBot.Entities.Http
{
    public class JDUserInfoUnionResult
    {
        public string msg { get; set; }

        public int retcode { get; set; }

        public JDUserInfoUnionResult_data data { get; set; }
    }

    public class JDUserInfoUnionResult_data
    {
        public JDUserInfoUnionResult_data_userInfo userInfo { get; set; }
        public JDUserInfoUnionResult_data_assetInfo assetInfo { get; set; }

    }

    public class JDUserInfoUnionResult_data_assetInfo
    {
        public int beanNum { get; set; }
    }

    public class JDUserInfoUnionResult_data_userInfo
    {

        public int isPlusVip { get; set; }

        public int isRealNameAuth { get; set; }

        public JDUserInfoUnionResult_data_userInfo_baseInfo baseInfo { get; set; }

    }
    public class JDUserInfoUnionResult_data_userInfo_baseInfo
    {
        public string levelName { get; set; }

        public string userLevel { get; set; }

        public string nickname { get; set; }
    }
}
