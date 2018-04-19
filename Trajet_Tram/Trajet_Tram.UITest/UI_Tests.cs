using System;
using NUnit.Framework;
using Trajet_Tram.UITest.Utils;

namespace Trajet_Tram.UITest
{
    [TestFixture]
    public class UI_Tests
    {
        Controller m_Controller;

        [SetUp]
        public void BeforeEachTest()
        {
            m_Controller = new Controller();

            try
            {
                Utils.Utils.ValidAutorisationIfNeeded(m_Controller);
            }
            catch (Exception) { }
        }


        [TestCase(TestName = "AppLaunches")]
        public void AppLaunches()
        {
            m_Controller.Screenshot("First screen.");
        }

        [TestCase("mesprit", "maxime.esprit@respawnsive.com", true, "Un nouveau mot de passe a été envoyé sur votre adresse email.", TestName = @"RetrievePassword_success")]
        [TestCase("mesprit", "maxime.esprom", false, "Vous devez renseigner une adresse email valide.", TestName = @"RetrievePassword_error_badEmail")]
        [TestCase("", "maxime.esprit@respawnsive.com", false, "Vous devez renseigner votre identifiant de compte BKU.", TestName = @"RetrievePassword_error_badMatriculeNumber")]
        public void RetrievePassword(string sLogin, string sEmail, bool bShouldBeSuccessfull, string sMsgExpected)
        {
            //Waits for the form to appear
            m_Controller.WaitVisible(Namemapping.activityLogin_relativeLayout_root);

            //Screenshot
            m_Controller.Screenshot("Appui sur bouton Nouveau Mot de Passe");
                        
            //Clicks the Login button
            m_Controller.Click(Namemapping.activityLogin_btn_askNewPassword);

            //Waits for the form to appear
            m_Controller.WaitVisible(Namemapping.activityLogin_cardview_newPassword);

            //Screenshot
            m_Controller.Screenshot("Saisie du login et de l'email");

            //Enter login and email
            m_Controller.SetText(Namemapping.activityLogin_textInputEditText_newPassword_matriculeNumber, sLogin, false);
            m_Controller.SetText(Namemapping.activityLogin_textInputEditText_newPassword_email, sEmail, false);

            //Screenshot
            m_Controller.Screenshot("Appui sur bouton valider");

            //Clicks the Valid button
            m_Controller.Click(Namemapping.activityLogin_btn_newPassword_valid);
            
            //bShouldBeSuccessfull = true => Verify Login form appear : the user has to be redirected to login form when new password has been asked successfully
            //bShouldBeSuccessfull = false => Verify Login forms doeasn't appear : the user must stay on the Ask New Password form 
            Assert.AreEqual(m_Controller.IsVisible(Namemapping.activityLogin_cardview_connection), bShouldBeSuccessfull);

            //Verify message
            Assert.AreEqual(m_Controller.VerifErrorMessage(sMsgExpected), true);

            //Screenshot
            m_Controller.Screenshot("Terminé avec succès.");
        }
    }
}

