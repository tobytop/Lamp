using System;

namespace Lamp.Core.Client.Token
{
    public interface IServiceTokenGetter
    {
        /// <summary>
        ///  获取凭证
        /// </summary>
        Func<string> GetToken { get; set; }
    }
}
