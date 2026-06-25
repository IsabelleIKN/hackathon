using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/*
 * Used to enable / disable Emitters / Receivers depending on whether or not this is a local avatar (controller by the user) or a remote avatar (controller by a different user or IVA).
 * Optional component I made to ease development.
 * 
 * author:
 * max@senseglove.com
 */

namespace Presence
{

    /// <summary> Enables / disables the right 'layer' on an avatar </summary>
    public class PHap_AvatarColliders : MonoBehaviour
    {
        /// <summary> If true, this is the avatar controlled by the local player.  </summary>
        [SerializeField] private bool isLocalClientAvatar = true;

        /// <summary> If true, we delete the unused scripts. </summary>
        [SerializeField] private bool destroyUnusedAssets = true;

        [SerializeField] List<PHap_HapticReceiver> avatarReceivers = new List<PHap_HapticReceiver>();

        [SerializeField] List<PHap_HapticEmitter> avatarEmitters = new List<PHap_HapticEmitter>();


        /// <summary> Components only used by the local avatar. They will be destroyed if isLocalClientAvatar = true and destroyUnusedAssets = true </summary>
        [SerializeField] List<Component> localAvatarAssets = new List<Component>();

        /// <summary> Components only used by the remove avatar. They will be destroyed if isLocalClientAvatar = false and destroyUnusedAssets = true </summary>
        [SerializeField] List<Component> remoteAvatarAssets = new List<Component>();


        private IEnumerator CheckAvatarStatus()
        {
            if (destroyUnusedAssets)
                yield return null; //If we're deleting, we do so on the next frame so someone has the opportunity to change their mind after spawning in a copy of the Avatar
            this.IsLocalAvatar = isLocalClientAvatar;
        }

        public void TryAddEmitter(PHap_HapticEmitter emitter)
        {
            if (emitter == null)
                return;
            if (!avatarEmitters.Contains(emitter))
            {
                avatarEmitters.Add(emitter);
            }
        }

        public void TryAddReceiver(PHap_HapticReceiver receiver)
        {
            if (receiver == null)
                return;
            if (!avatarReceivers.Contains(receiver))
            {
                avatarReceivers.Add(receiver);
            }
        }

        public bool IsLocalAvatar
        {
            get
            {
                return isLocalClientAvatar;
            }
            set
            {
                isLocalClientAvatar = value;

                if (destroyUnusedAssets)
                {
                    if (isLocalClientAvatar)
                    {
                        foreach (PHap_HapticEmitter emitt in avatarEmitters) //emitters only enabled on remove avatar(s)
                            GameObject.Destroy(emitt);
                        foreach (PHap_HapticReceiver receiver in avatarReceivers) //receivers only enabled on local avatar
                            receiver.enabled = true;
                        foreach (Component comp in remoteAvatarAssets)
                        {
                            if (comp is Transform)
                                GameObject.Destroy(comp.gameObject);
                            else
                                GameObject.Destroy(comp);
                        }
                    }
                    else
                    {
                        foreach (PHap_HapticEmitter emitt in avatarEmitters) //emitters only enabled on remove avatar(s)
                            emitt.enabled = true;
                        foreach (PHap_HapticReceiver receiver in avatarReceivers) //receivers only enabled on local avatar
                            GameObject.Destroy(receiver);
                        foreach (Component comp in localAvatarAssets)
                        {
                            if (comp is Transform)
                                GameObject.Destroy(comp.gameObject);
                            else
                                GameObject.Destroy(comp);
                        }
                    }
                }
                else
                {
                    foreach (PHap_HapticEmitter emitt in avatarEmitters) //emitters only enabled on remove avatar(s)
                        emitt.enabled = !isLocalClientAvatar;
                    foreach (PHap_HapticReceiver receiver in avatarReceivers) //receivers only enabled on local avatar
                        receiver.enabled = isLocalClientAvatar;
                }
            }
        }


        // Start is called before the first frame update
        void Start()
        {
            StartCoroutine( CheckAvatarStatus() );
        }


    }
}
