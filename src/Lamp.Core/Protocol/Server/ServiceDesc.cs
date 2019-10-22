﻿using System;
using System.Collections.Concurrent;
using System.Collections.Generic;

namespace Lamp.Core.Protocol.Server
{
    public class ServiceDesc : ICloneable
    {
        public ServiceDesc()
        {
            Metadatas = new ConcurrentDictionary<string, object>();
        }

        /// <summary>
        /// 服务ID
        /// </summary>
        public string Id
        {
            get => GetMetadata<string>("Id");
            set => Metadatas["Id"] = value;
        }

        /// <summary>
        /// 服务路径
        /// </summary>
        public string RoutePath
        {
            get => GetMetadata<string>("RoutePath");
            set => Metadatas["RoutePath"] = value;
        }

        public bool WaitExecution
        {
            get => GetMetadata<bool>("WaitExecution");
            set => Metadatas["WaitExecution"] = value;
        }

        public bool EnableAuthorization
        {
            get => GetMetadata<bool>("EnableAuthorization");
            set => Metadatas["EnableAuthorization"] = value;
        }

        public string CreatedDate
        {
            get => GetMetadata<string>("CreatedDate");
            set => Metadatas["CreatedDate"] = value;
        }

        public string CreatedBy
        {
            get => GetMetadata<string>("CreatedBy");
            set => Metadatas["CreatedBy"] = value;
        }

        public string Comment
        {
            get => GetMetadata<string>("Comment");
            set => Metadatas["Comment"] = value;
        }

        public string Roles
        {
            get => GetMetadata<string>("Roles");
            set => Metadatas["Roles"] = value;
        }

        public string ReturnDesc
        {
            get => GetMetadata<string>("ReturnDesc");
            set => Metadatas["ReturnDesc"] = value;
        }

        public string Parameters
        {
            get => GetMetadata<string>("Parameters");
            set => Metadatas["Parameters"] = value;
        }

        public string HttpMethod
        {
            get => GetMetadata<string>("HttpMethod");
            set => Metadatas["HttpMethod"] = value;
        }

        public string Version
        {
            get => GetMetadata<string>("Version");
            set => Metadatas["Version"] = value;
        }
        /// <summary>
        ///     other useful data
        /// </summary>
        public IDictionary<string, object> Metadatas { get; set; }

        public T GetMetadata<T>(string name, T def = default(T))
        {
            if (!Metadatas.ContainsKey(name))
            {
                return def;
            }

            return (T)Metadatas[name];
        }
        public object Clone()
        {
            return MemberwiseClone();
        }
    }
}
