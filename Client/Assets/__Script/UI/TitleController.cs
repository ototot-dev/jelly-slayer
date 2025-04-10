using UnityEngine;
using UnityEngine.UI;
using UniRx;
using UGUI.Rx;

namespace Game
{
    [Template(path: "UI/title")]
    public class TitleController : Controller
    {
        public override void OnPreShow()
        {
            base.OnPreShow();

            GameContext.Instance.MainCanvasCtrler.FadeInImmediately(Color.black);
        }

        public override void OnPostShow()
        {
            base.OnPostShow();

            LoadingPageController loadingCtrler = null;

            template.GetComponentById<Button>("start").OnClickAsObservable().Where(_ => loadingCtrler == null).Subscribe(_ =>
            {
                this.Hide().Unload();
                loadingCtrler = new LoadingPageController().Load();
                loadingCtrler.Show(GameContext.Instance.MainCanvasCtrler.body);
            }).AddToHide(this);
        }
    }
}