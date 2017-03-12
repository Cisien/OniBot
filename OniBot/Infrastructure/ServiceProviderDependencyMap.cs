using Discord.Commands;
using Microsoft.Extensions.DependencyInjection;
using System;

namespace OniBot.Infrastructure
{
    public class ServiceProviderDependencyMap : IDependencyMap
    {
        IServiceCollection _services;
        IServiceProvider _provider;

        public ServiceProviderDependencyMap()
        {
            _services = new ServiceCollection();
            _provider = _services.BuildServiceProvider();
        }

        public IServiceCollection GetCollection()
        {
            return _services;
        }

        public IServiceProvider GetProvider()
        {

            _provider = _services.BuildServiceProvider();
            return _provider;
        }

        public void Add(Type type, object impl)
        {
            _services.AddSingleton(type, impl);
        }

        /// <inheritdoc />
        public void Add<T>(T obj) where T : class
        {
            AddFactory(() => obj);
        }
        /// <inheritdoc />
        public bool TryAdd<T>(T obj) where T : class
        {
            return TryAddFactory(() => obj);
        }
        /// <inheritdoc />
        public void AddTransient<T>() where T : class, new()
        {
            _services.AddTransient<T>();
        }
        /// <inheritdoc />
        public bool TryAddTransient<T>() where T : class, new()
        {
            _services.AddTransient<T>();
            return true;
        }
        public void AddTransient(Type type)
        {
            _services.AddTransient(type);
        }

        public void AddTransientFactory(Type type, Func<object> factory)
        {
            _services.AddTransient(type, (sp) => factory());
        }

        /// <inheritdoc />
        public void AddTransient<TKey, TImpl>() where TKey : class where TImpl : class, TKey, new()
        {
            _services.AddTransient<TKey, TImpl>();
        }
        public bool TryAddTransient<TKey, TImpl>() where TKey : class where TImpl : class, TKey, new()
        {
            _services.AddTransient<TKey, TImpl>();
            return true;
        }

        /// <inheritdoc />
        public void AddFactory<T>(Func<T> factory) where T : class
        {
            _services.AddSingleton(factory());
        }
        /// <inheritdoc />
        public bool TryAddFactory<T>(Func<T> factory) where T : class
        {
            _services.AddSingleton(factory());
            return true;
        }


        /// <inheritdoc />
        public T Get<T>()
        {
            return (T)Get(typeof(T));
        }
        /// <inheritdoc />
        public object Get(Type t)
        {
            object ret = _provider.GetService(t);

            if (ret == null)
            {
                _provider = _services.BuildServiceProvider();
                ret = _provider.GetService(t);
            }

            return ret;
        }

        /// <inheritdoc />
        public bool TryGet<T>(out T result)
        {
            object untypedResult;
            if (TryGet(typeof(T), out untypedResult))
            {
                result = (T)untypedResult;
                return true;
            }
            else
            {
                result = default(T);
                return false;
            }
        }
        /// <inheritdoc />
        public bool TryGet(Type t, out object result)
        {
            try
            {
                result = Get(t);
                return true;
            }
            catch (Exception)
            {
                result = null;
                return false;
            }
        }
    }
}
