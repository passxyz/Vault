using System;
using System.Collections.Generic;
using System.Text;

using Xamarin.Forms;
using Xamarin.Essentials;

using FontAwesome.Solid;

namespace PassXYZLib
{
    /// <summary>
    /// This is a class extended PassXYZLib.User. It has a dependency on Xamarin.Forms.
    /// </summary>
    public class PxUser : User
    {
        /// <summary>
        /// The DefaultTimeout is set to 120 seconds.
        /// If this is too short, there is problem to apply sync package.
        /// </summary>
        public static int DefaultTimeout = 120;

        /// <summary>
        /// The Timeout value to close the database.
        /// </summary>
        public static int AppTimeout
        {
            get => Preferences.Get(nameof(AppTimeout), DefaultTimeout);

            set => Preferences.Set(nameof(AppTimeout), value);
        }

        /// <summary>
        /// This is the icon for the user. We will use an icon to differentiate the user with Device Lock.
        /// </summary>
        public ImageSource ImgSource => new IconSource
        {
            Icon = IsDeviceLockEnabled ? Icon.UserLock : Icon.User
        };

    }
}
