using Android.App;
using Android.Content.PM;
using Android.Support.V4.App;
using Android.Support.V4.Content;

namespace Trajet_Tram.Droid.Utils
{
    public static class AskForPermissions
    {
        public static bool askForWriteExternalStoragePermission(Activity context, int iRequestID = 0, string sMessage = null)
        {
            bool bIsGranted = isWriteExternalStoragePermissionGranted(context);

            if (sMessage == null)
                sMessage = context.Resources.GetString(Resource.String.askForPermissions_WriteExternalStorage);

            //If permission is not granted, ask user to grant it.
            if (!bIsGranted)
            {
                Android.Support.V7.App.AlertDialog dialog = new Android.Support.V7.App.AlertDialog.Builder(context)
                    .SetTitle(context.Resources.GetString(Resource.String.app_name))
                    .SetMessage(sMessage)
                    .SetPositiveButton(context.Resources.GetString(Resource.String.snackbar_OK), delegate
                    {
                        string[] lsPermission = { Android.Manifest.Permission.WriteExternalStorage };

                        ActivityCompat.RequestPermissions(context, lsPermission, iRequestID);
                    })
                    .SetIcon(Resource.Drawable.Icon)
                    .SetCancelable(false)
                    .Show();
            }

            return bIsGranted;
        }

        public static bool isWriteExternalStoragePermissionGranted(Activity context)
        {
            return context != null && ContextCompat.CheckSelfPermission(context, Android.Manifest.Permission.WriteExternalStorage) == Permission.Granted;
        }

        public static bool askForGPSPermission(Activity context, int iRequestID = 0)
        {
            bool bIsGranted = isGPSPermissionGranted(context);

            //If permission is not granted, ask user to grant it.
            if (!bIsGranted)
            {
                Android.Support.V7.App.AlertDialog dialog = new Android.Support.V7.App.AlertDialog.Builder(context)
                    .SetTitle(context.Resources.GetString(Resource.String.app_name))
                    .SetMessage(Resource.String.askForPermissions_AccessFineLocation)
                    .SetPositiveButton(context.Resources.GetString(Resource.String.snackbar_OK), delegate
                    {
                        string[] lsPermission = { Android.Manifest.Permission.AccessCoarseLocation, Android.Manifest.Permission.AccessFineLocation };

                        ActivityCompat.RequestPermissions(context, lsPermission, iRequestID);
                    })
                    .SetIcon(Resource.Drawable.Icon)
                    .SetCancelable(false)
                    .Show();
            }

            return bIsGranted;
        }

        public static bool isGPSPermissionGranted(Activity context)
        {
            return context != null && ContextCompat.CheckSelfPermission(context, Android.Manifest.Permission.AccessCoarseLocation) == Permission.Granted && ContextCompat.CheckSelfPermission(context, Android.Manifest.Permission.AccessFineLocation) == Permission.Granted;
        }

    }
}