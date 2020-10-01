﻿namespace CascadeWorker.Config
{
    public interface IConfigProvider
    {
        void Load(string url);
        string GetValueByKey(string key);
    }
}