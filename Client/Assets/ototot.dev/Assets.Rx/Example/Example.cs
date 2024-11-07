using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UniRx;
using Assets.Rx;


public class Example : MonoBehaviour
{
    // Start is called before the first frame update
    void Start() {        
        // AssetsConfig.Instance.remoteUrl = "file:///Users/pp/Documents/dev/Assets.Rx/Build/AssetBundle";

        AssetsManager.Instance.Init()
            .DoOnError(e =>
                Debug.LogWarningFormat("AssetsManager init failed!!. error code => {0}", e.ToString())
            )			
            .Subscribe(_ =>
                Debug.Log("AssetsManager inited~")
            );
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

// // namespace Assets.Rx {

// 	public class AssetsRxExample : MonoBehaviour {

// 		public AssetRef<Texture2D> myTex;

// 		void Start () {
// 			AssetsConfig.Instance.remoteUrl = "file:///Users/pp/Documents/dev/Assets.Rx/Assets/../Build/AssetBundle";

// 			AssetsManager.Instance.Init()
// 				.DoOnError(e =>
// 					Debug.LogWarningFormat("AssetsManager init failed!!. error code => {0}", e.ToString())
// 				)			
// 				.Subscribe(_ =>
// 					Debug.Log("AssetsManager inited~")
// 				);

			


// 			// 	AssetsManager.Instance.GetByPath<Texture2D>("__Data/images/Image_Ads")
// 			// 		.Subscribe(v => {
// 			// 			myTex = v;
// 			// 		});
// 			// 	}

// 			// Observable.Timer(TimeSpan.FromSeconds(4f))
// 			// 	.Subscribe(_ => {
// 			// 		myTex.Dispose();
// 			// 		myTex = null;
// 			// 	});

// 			// Observable.Timer(TimeSpan.FromSeconds(1f)).Subscribe(_ => {
// 			// 	AssetsManager.Instance.GetByPath<Sprite>("__Data/images/Image_Ads")
//             //         .Subscribe(vv => {
//             //             if (vv != null)
//             //                 Debug.LogFormat("{0} is loaded", vv.name);
//             //         });
// 			// });
			
// 		}
// 	}	
// // }

