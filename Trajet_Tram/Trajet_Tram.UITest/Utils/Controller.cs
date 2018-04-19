using System;
using System.IO;
using System.Linq;
using Xamarin.UITest.Queries;

namespace Trajet_Tram.UITest.Utils
{
    public class Controller
    {
        /*
         * Message: System.Exception : ApkFile does not exist: C:\Program Files (x86)\Microsoft Visual Studio\2017\Trajet_Tram.Android\bin\Debug\com.respawnsive.TrajetTram.apk
         */

        // => se referrer à https://forums.xamarin.com/discussion/26112/uitest-on-android-how-to-run-it-natively
        private const string sAPKFile = @"..\..\..\..\..\TrajetTram\Trajet_Tram\Trajet_Tram.Android\bin\Debug\com.respawnsive.TrajetTram.apk";
        private string m_sOutputPath;
        private Xamarin.UITest.Android.AndroidApp m_App;


        /// <summary>
        /// Start the android app.
        /// </summary>
        public Controller()
        {
            m_App = Xamarin.UITest.ConfigureApp.Android.EnableLocalScreenshots().ApkFile(sAPKFile).StartApp();
            m_sOutputPath = Path.Combine(Path.GetDirectoryName(sAPKFile), "Output", DateTime.Now.ToString("u").Replace(":", "-"));

            try
            {
                Directory.CreateDirectory(m_sOutputPath);
            }
            catch (System.UnauthorizedAccessException)
            {
                //nothing to do : on mobile center we are unable to create file but it's working on local machines.
            }
        }

        /// <summary>
        /// Return the AppResult object of the control which ID = sIDOfControl.
        /// See the documentation https://developer.xamarin.com/api/type/Xamarin.UITest.Queries.AppResult/
        /// </summary>
        /// <param name="sIDOfControl">String. Id of the specified control.</param>
        /// <returns>AppResult. The first corresponding object.</returns>
        private AppResult GetControlFromID(string sIDOfControl)
        {
            AppResult[] res = m_App.Query(c => c.Marked(sIDOfControl));

            return res[0];
        }

        /// <summary>
        /// Take a screenshot and copy the png file to the Output directory.
        /// </summary>
        /// <param name="sTitle">String. Title of the screenshot.</param>
        public void Screenshot(string sTitle)
        {
            FileInfo fScreenshot = m_App.Screenshot(sTitle);
            //fScreenshot.MoveTo(Path.Combine(m_sOutputPath, sTitle + "_" + DateTime.Now.ToString("HH_mm_ss_f") + ".png"));
        }

        /// <summary>
        /// Perform a Tap on the control which ID = sIDOfControlToClick
        /// </summary>
        /// <param name="sIDOfControlToClick">String. Id of the control to Tap on.</param>
        public void Click(string sIDOfControlToClick)
        {
            WaitVisible(sIDOfControlToClick, true, 500);

            m_App.Tap(c => c.Marked(sIDOfControlToClick));
        }
        
        public void Toggle_Checkbox(string sIDOfCheckBox, bool bChecked)
        {
            WaitVisible(sIDOfCheckBox, true, 500);

            m_App.Query(c => c.Id(sIDOfCheckBox).Invoke("setChecked", bChecked));
        }


        /// <summary>
        /// Enter text into the control which ID = sIDOfControlToEdit.
        /// </summary>
        /// <param name="sIDOfControlToEdit">String. Id of the control to enter text.</param>
        /// <param name="sText">String. Text to enter.</param>
        /// <param name="bDontClearText">Boolean. If true, doesn't clear the current text of the element and append sText at the end of the existing text.</param>
        public void SetText(string sIDOfControlToEdit, string sText, bool bDontClearText = true)
        {
            WaitVisible(sIDOfControlToEdit, true, 500);

            if (!bDontClearText)
                while (GetText(sIDOfControlToEdit) != "")
                    m_App.ClearText(c => c.Marked(sIDOfControlToEdit));

            m_App.EnterText(c => c.Marked(sIDOfControlToEdit), sText);
        }

        /// <summary>
        /// Return the text of the control which ID = sIDOfControlToGetText
        /// </summary>
        /// <param name="sIDOfControlToEdit">String. Id of the control to get the text.</param>
        /// <returns>String. The text of the specified control.</returns>
        public string GetText(string sIDOfControlToGetText)
        {
            WaitVisible(sIDOfControlToGetText);

            AppResult control = GetControlFromID(sIDOfControlToGetText);

            return control.Text;
        }

        internal void OpenNavigationView()
        {
            //m_App.TapCoordinates(75, 75); Working on Nexus 9, not on GalaxyTab4...
            m_App.DragCoordinates(0, 500, 500, 500);
        }

        /// <summary>
        /// Press the button "More" on the Top Right corner of the screen
        /// </summary>
        public void PressAndroidMoreOptionMenu()
        {
            m_App.Tap(x => x.ClassFull("android.support.v7.widget.ActionMenuPresenter$OverflowMenuButton"));
        }

        public void PressEnter()
        {
            m_App.PressEnter();
        }

        /// <summary>
        /// Wait for the control which ID = sIDOfControlToWaitFor. If the control doesn't appear/disappear, throw a System.TimeoutException.
        /// </summary>
        /// <param name="sIDOfControlToWaitFor">String. Id of the control to wait for.</param>
        /// <param name="bVisible">Boolean. If true, waits until the control appears on the screen. If false, waits until the control disappears from the screen.</param>
        /// <param name="iTimeOut">Integer. Max time to wait, in millisecond.</param>
        /// <returns></returns>
        public void WaitVisible(string sIDOfControlToWaitFor, bool bVisible = true, int iTimeOut = 3000)
        {
            //If bVisible == true
            //If sIDOfControlToWaitFor == visible 
            //=> OK
            //Else
            //ScrollTo sIDOfControlToWaitFor
            //If sIDOfControlToWaitFor == visible 
            //=> OK
            //Else
            //=> KO

            string sTimeoutMessage = "Timeout : control " + sIDOfControlToWaitFor + " " + (bVisible ? "is not visible" : "is still visible") + " after " + iTimeOut / 1000 + " seconds.";
            TimeSpan tTimeOut = TimeSpan.FromMilliseconds(iTimeOut);

            try
            {
                if (bVisible)
                {
                    bool bVis = false;
                    try
                    {
                        m_App.WaitForElement(c => c.Marked(sIDOfControlToWaitFor), sTimeoutMessage, tTimeOut);
                        bVis = true;
                    }
                    catch (Exception)
                    {
                        ScrollTo(sIDOfControlToWaitFor);
                    }

                    if (!bVis)
                        m_App.WaitForElement(c => c.Marked(sIDOfControlToWaitFor), sTimeoutMessage, TimeSpan.FromMilliseconds(2000));
                }
                else
                    m_App.WaitForNoElement(c => c.Marked(sIDOfControlToWaitFor), sTimeoutMessage, tTimeOut);
            }
            catch (Exception e)
            {
                Screenshot(sTimeoutMessage);
                throw e;
            }
        }

        private void ScrollTo(string sIDOfControlToScrollTo, int iTimeOut = 2000)
        {
            try
            {
                m_App.ScrollTo(c => c.Marked(sIDOfControlToScrollTo));
            }
            catch (Exception e)
            { }
        }

        /// <summary>
        /// Checks if a control is visible. If yes, checks wether a control is enable or not.
        /// </summary>
        /// <param name="sIDOfControlToCheckEnable">String. ID of the control to check.</param>
        /// <returns>Boolean. True if the control is enable, else false</returns>
        public bool IsEnable(string sIDOfControlToCheckEnable)
        {
            WaitVisible(sIDOfControlToCheckEnable);

            AppResult control = GetControlFromID(sIDOfControlToCheckEnable);

            return control.Enabled;
        }

        /// <summary>
        /// Checks if a control is visible.
        /// </summary>
        /// <param name="sIDOfControlToCheckVisibility">String. ID of the control to check.</param>
        /// <returns>Boolean. True if the control is visible, else false</returns>
        public bool IsVisible(string sIDOfControlToCheckVisibility)
        {
            try
            {
                m_App.WaitForElement(c => c.Marked(sIDOfControlToCheckVisibility), "", TimeSpan.FromSeconds(3));
                return true;
            }
            catch (TimeoutException e)
            {
                return false;
            }
            catch(Exception ex)
            {
                //If the element was not visible before and after the 3 seconds, this exception is thrown. Usefull to verify a windows does NOT appear after a click.
                if (ex.Source == "Xamarin.UITest" && ex.Message.Contains("Error while performing WaitForElement"))
                    return false;
                else
                    throw ex;
            }
        }

        /// <summary>
        /// Checks if the error message is on screen.
        /// </summary>
        /// <param name="sTextOfTheError">String. Text of the error message</param>
        /// <returns>Boolean. True if the message is on screen.</returns>
        public bool VerifErrorMessage(string sTextOfTheError)
        {
            AppResult[] res = m_App.Query(c => c.Marked(sTextOfTheError));
            return res.Count() != 0;
        }

        public void Repl()
        {
            m_App.Repl();
        }
    }
}
