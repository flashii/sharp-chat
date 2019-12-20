﻿namespace SharpChat.Flashii {
    public static class FlashiiUrls {
        public const string BASE_URL =
#if _DEBUG
            @"http://msz.lh/_sockchat";
#else
            @"https://flashii.net/_sockchat";
#endif

        public const string AUTH = BASE_URL + @"/verify";
        public const string BANS = BASE_URL + @"/bans";
        public const string BUMP = BASE_URL + @"/bump";
    }
}
