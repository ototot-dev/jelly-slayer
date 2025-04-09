using System;
using System.Collections.Generic;
using UGUI.Rx;

namespace Game
{
    [Template(path: "UI/template/game-panel")]
    public class GamePanelController : Controller
    {
        public override void OnPreLoad(List<IObservable<Controller>> loader)
        {
            base.OnPreLoad(loader);
        }

        public override void OnPreShow()
        {
            base.OnPreShow();
        }
    }
}