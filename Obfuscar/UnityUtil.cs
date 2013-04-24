using System;
using System.Collections.Generic;
using System.Text;

namespace Obfuscar
{
    class UnityUtil
    {
        static public List<string> skipMethods = new List<string>{"Update", 
                                                                    "LateUpdate", 
                                                                    "FixedUpdate",
                                                                    "Awake", 
                                                                    "Start",	
                                                                    "Reset",	
                                                                    "OnMouseEnter",	
                                                                    "OnMouseOver",	 
                                                                    "OnMouseExit",	
                                                                    "OnMouseDown",	 
                                                                    "OnMouseUp",	 
                                                                    "OnMouseUpAsButton", 
                                                                    "OnMouseDrag",	
                                                                    "OnTriggerEnter",	
                                                                    "OnTriggerExit",	
                                                                    "OnTriggerStay",	
                                                                    "OnCollisionEnter",	
                                                                    "OnCollisionExit",	 
                                                                    "OnCollisionStay",	 
                                                                    "OnControllerColliderHit",	 
                                                                    "OnJointBreak",	
                                                                    "OnParticleCollision",	
                                                                    "OnBecameVisible",	
                                                                    "OnBecameInvisible",	
                                                                    "OnLevelWasLoaded",	
                                                                    "OnEnable",	
                                                                    "OnDisable",	
                                                                    "OnDestroy",	
                                                                    "OnPreCull",	
                                                                    "OnPreRender",	
                                                                    "OnPostRender",	
                                                                    "OnRenderObject",	
                                                                    "OnWillRenderObject",	
                                                                    "OnGUI",	
                                                                    "OnRenderImage",	
                                                                    "OnDrawGizmosSelected",	
                                                                    "OnDrawGizmos",	
                                                                    "OnApplicationPause", 
                                                                    "OnApplicationFocus",	
                                                                    "OnApplicationQuit",	 
                                                                    "OnPlayerConnected",	
                                                                    "OnServerInitialized",	
                                                                    "OnConnectedToServer",	
                                                                    "OnPlayerDisconnected",	
                                                                    "OnDisconnectedFromServer",	
                                                                    "OnFailedToConnect",	
                                                                    "OnFailedToConnectToMasterServer",	
                                                                    "OnMasterServerEvent",	 
                                                                    "OnNetworkInstantiate",	
                                                                    "OnSerializeNetworkView",	
                                                                    "OnAudioFilterRead",	 
                                                                    "OnAnimatorMove",	
                                                                    "OnAnimatorIK "};

        static public bool isSkipMethod(string methodString)
        {
            foreach (string unityMethod in skipMethods)
                if (methodString.Contains(unityMethod))
                    return true;
            
            return false;
        }       
	 
    }
}
