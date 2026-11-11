using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Oathx.Core
{
    public abstract class XPlugin : XEventDispatcher
    {
        /// <summary>
        /// install plugin internal resources
        /// </summary>
        /// <returns></returns>
        public abstract bool Install();

        /// <summary>
        /// 
        /// </summary>
        /// <returns></returns>
        public abstract bool Startup();

        /// <summary>
        /// 
        /// </summary>
        public abstract void Shutdown();

        /// <summary>
        /// uninstall plugin internal resuources
        /// </summary>
        public abstract void Uninstall();
    }
}
