using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Trajet_Tram.UITest.Utils
{
    public static class Utils
    {
        /// <summary>
        /// Once you are in the home page, call this method to open the more menu on the top right and click the "add test trajet" entry
        /// </summary>
        /// <param name="ctrl">Controller</param>
        public static void AddTesttrajet(Controller ctrl)
        {
            ctrl.PressAndroidMoreOptionMenu();
            ctrl.Click(Namemapping.dashboardActivity_menu_addtrajetTest);
        }
        
        /// <summary>
        /// Call this method to click on OK button in Android alert dialog that ask authorizations
        /// </summary>
        /// <param name="ctrl">Controller</param>
        /// <returns>Task</returns>
        public static async Task ValidAutorisationIfNeeded(Controller ctrl)
        {
            ctrl.Click(Namemapping.activityLogin_autorizationAlertDialog_btn_valid);
            ctrl.Click(Namemapping.activityLogin_autorizationAndroidAlertDialog_btn_valid);
        }
    }
}
