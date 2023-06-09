using Android.App;
using Android.Content;
using Android.Gms.Extensions;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using AndroidX.AppCompat.App;
using Firebase;
using Firebase.Auth;
using Firebase.Firestore;
using System.Threading.Tasks;

namespace Bonganciso_MyApp_FlashBox
{
    [Activity(Label = "@string/app_name", Theme = "@style/AppTheme", MainLauncher = true)]
    public class MainActivity : AppCompatActivity
    {
        EditText textFName, textLName, textEmail, textUsername, textPassword, loginemail, loginpass;
        Button signup, signin;
        string fname, lname, email, username, password;
        FirebaseFirestore db;
        FirebaseAuth auth;
        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.activity_main);

            FirebaseApp.InitializeApp(this);
            // Get the Firestore instance
            db = FirebaseFirestore.Instance;

            auth = FirebaseAuth.GetInstance(FirebaseApp.Instance);

           
            Button signinbtn = FindViewById<Button>(Resource.Id.btnsignin);
            signinbtn.Click += signinbtn_Click;
        }

        private void signinbtn_Click(object sender, System.EventArgs e)
        {
            ShowSigninDialog();
        }

        private void ShowSigninDialog()
        {
            // Create a dialog with no title
            Dialog dialog = new Dialog(this);
            dialog.RequestWindowFeature((int)WindowFeatures.NoTitle);

            // Set the content view to your custom dialog layout
            dialog.SetContentView(Resource.Layout.signin);

            // Adjust dialog properties to occupy the entire screen
            Window window = dialog.Window;
            window.SetLayout(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent);

            loginemail = dialog.FindViewById<EditText>(Resource.Id.getUsername);
            loginpass = dialog.FindViewById<EditText>(Resource.Id.getPassword);
            TextView tosignup = dialog.FindViewById<TextView>(Resource.Id.gotosignup);
            tosignup.Click += (sender, e) =>
            {
                ShowSignupDialog();
                dialog.Dismiss();
            };
            signin = dialog.FindViewById<Button>(Resource.Id.signinAcc);
            signin.Click += async (sender, e) =>
            {
                string check1 = loginemail.Text;
                string check2 = loginpass.Text;
                bool isValid = ValidateField(check1, check2);
                if (isValid)
                {
                    username = loginemail.Text;
                    password = loginpass.Text;

                    if (username.Contains("@"))
                    {
                        bool isEmailValid = await AuthenticateUser(username, password);
                        if (isEmailValid)
                        {
                            string documentId = await GetUsernameFromEmail(username);
                            if (!string.IsNullOrEmpty(documentId))
                            {
                                bool isCredentialsValid = await ValidateCredentials(documentId, password);
                                if (isCredentialsValid)
                                {
                                    Intent intent = new Intent(this, typeof(Homepage));
                                    intent.PutExtra("Username", documentId);
                                    StartActivity(intent);
                                    EmptyField(loginemail, loginpass);
                                }
                                else
                                {
                                    Toast.MakeText(this, "Invalid Email or Password", ToastLength.Short).Show();
                                    EmptyField(loginemail, loginpass);
                                }
                            }
                            else
                            {
                                // Document with the provided email not found
                                Toast.MakeText(this, "Email not found", ToastLength.Short).Show();
                                EmptyField(loginemail, loginpass);
                            }
                        }
                        else
                        {
                            Toast.MakeText(this, "Invalid Email or Password", ToastLength.Short).Show();
                            EmptyField(loginemail, loginpass);
                        }
                    }
                    else
                    {
                        bool isCredentialsValid = await ValidateCredentials(username, password);
                        if (isCredentialsValid)
                        {
                            Intent intent = new Intent(this, typeof(Homepage));
                            intent.PutExtra("Username", username);
                            StartActivity(intent);
                            EmptyField(loginemail, loginpass);
                        }
                        else
                        {
                            EmptyField(loginemail, loginpass);
                        }
                    }
                }
            };
            dialog.Show();
        }

        private async Task<string> GetUsernameFromEmail(string email)
        {
            var querySnapshot = await db.Collection("User").WhereEqualTo("Email", email).Get().AsAsync<QuerySnapshot>();

            if (querySnapshot.Documents.Count > 0)
            {
                var documentSnapshot = querySnapshot.Documents[0];
                return documentSnapshot.Id;
            }
            else
            {
                // Document with the provided email not found
                return string.Empty;
            }
        }


        private void ShowSignupDialog()
        {

            // Create the dialog
            Dialog dialog = new Dialog(this);

            // Set the custom layout for the dialog
            dialog.SetContentView(Resource.Layout.signup);

            // Set the dialog to occupy the full screen
            dialog.Window.SetLayout(WindowManagerLayoutParams.MatchParent, WindowManagerLayoutParams.MatchParent);

            textFName = dialog.FindViewById<EditText>(Resource.Id.textFName);
            textLName = dialog.FindViewById<EditText>(Resource.Id.textLName);
            textEmail = dialog.FindViewById<EditText>(Resource.Id.textEmail);
            textUsername = dialog.FindViewById<EditText>(Resource.Id.textUsername);
            textPassword = dialog.FindViewById<EditText>(Resource.Id.textPassword);
            TextView tosignin = dialog.FindViewById<TextView>(Resource.Id.gotosignin);
            tosignin.Click += (sender, e) =>
            {
                ShowSigninDialog();
                dialog.Dismiss();
            };
            signup = dialog.FindViewById<Button>(Resource.Id.signupAcc);
            signup.Click += async (sender, e) =>
            {
                fname = textFName.Text;
                lname = textLName.Text;
                email = textEmail.Text;
                username = textUsername.Text;
                password = textPassword.Text;

                bool isValid = ValidateField(fname, lname, email, username, password); //check if all field is empty

                if (isValid == true) //all fields are all filled im
                {
                    if (textEmail.Text.Contains("@") && !string.IsNullOrEmpty(email)) //email is correct
                    {
                        if (password.Length >= 8)
                        {
                            bool isValidUsername = await ValidateUsername(username);
                            if (isValidUsername == false) //the username does not exists; username is unique
                            {
                                bool checkemail = await AuthenticateUser(email, password);
                                if (checkemail == false)
                                {
                                    //register email
                                    RegisterEmail(email, password);
                                    DocumentReference docRef = db.Collection("User").Document(username);

                                    // Create a data object to save
                                    var data = new JavaDictionary<string, Java.Lang.Object>
                                        {
                                            { "FirstName", new Java.Lang.String(fname) },
                                            { "LastName", new Java.Lang.String(lname) },
                                            { "Email", new Java.Lang.String(email) },
                                            { "Username", new Java.Lang.String(username) },
                                            { "Password", new Java.Lang.String(password) },

                                         };

                                    // Save the document to Firestore
                                    await docRef.Set(data);


                                    // Display a toast message

                                    Toast.MakeText(this, "User added", ToastLength.Short).Show();

                                    EmptyField(textFName, textLName, textEmail, textUsername, textPassword);
                                    dialog.Dismiss();
                                    ShowSigninDialog();
                                    //this should lead to login
                                    //login is the same as this nga customize dialog

                                }
                                else
                                {
                                    Toast.MakeText(this, "Email already exists", ToastLength.Short).Show();
                                    textEmail.Text = string.Empty;
                                }
                            }
                            else
                            {
                                Toast.MakeText(this, "Username already exists", ToastLength.Short).Show();
                            }

                        }
                        else
                        {
                            Toast.MakeText(this, "Password should be atleast 8 or more characters", ToastLength.Short).Show();
                            textPassword.Text = string.Empty;
                        }
                    }
                    else
                    {
                        Toast.MakeText(this, "Invalid email", ToastLength.Short).Show();
                        textEmail.Text = string.Empty;

                    }

                }
            };
            // Show the dialog
            dialog.Show();

        }

        public bool ValidateField(params string[] fields)
        {
            // Iterate through each field and check if it's not empty
            foreach (string field in fields)
            {
                if (string.IsNullOrEmpty(field))
                {
                    Toast.MakeText(this, "Fill all fields", ToastLength.Short).Show();
                    return false; // Return false if any field is empty
                }
            }

            return true; // Return true if all fields are not empty
        }
        public async Task<bool> ValidateUsername(string username)
        {
            var document = db.Collection("User").Document(username); // Set the document ID as the username

            var documentSnapshot = await document.Get().AsAsync<DocumentSnapshot>();

            if (documentSnapshot.Exists())
            {
                return true;
            }
            else
            {
                return false;
            }
        }

        public async Task<bool> ValidateCredentials(string username, string password)
        {
            var document = db.Collection("User").Document(username); // Set the document ID as the username

            var documentSnapshot = await document.Get().AsAsync<DocumentSnapshot>();

            if (documentSnapshot.Exists())
            {
                // Retrieve the password field value from the document snapshot
                string storedPassword = documentSnapshot.Get("Password").ToString();

                // Compare the stored password with the inputted password
                if (password == storedPassword)
                {
                    return true;
                }
                else
                {
                    
                    Toast.MakeText(this, "Invalid Password", ToastLength.Short).Show();
                    return false;
                }
            }
            else
            {
               
                Toast.MakeText(this, "Username does not exists", ToastLength.Short).Show();
                return false;
            }

            
        }

        private async void RegisterEmail(string email, string password)
        {
            try
            {
                var result = await auth.CreateUserWithEmailAndPasswordAsync(email, password);
                var user = result.User;
            }
            catch (FirebaseAuthException)
            {
                Toast.MakeText(this, "Failed to register user", ToastLength.Short).Show();
            }
        }

        private async Task<bool> AuthenticateUser(string username, string password)
        {
            try
            {
                var result = await auth.SignInWithEmailAndPasswordAsync(username, password);
                var user = result.User;
                return true;
            }
            catch (FirebaseAuthException)
            {
                return false;
            }
        }

        public void EmptyField(params EditText[] fields)
        {
            // Iterate through each field and check if it's not empty
            foreach (EditText field in fields)
            {
                field.Text = string.Empty;
            }

        }


        public override void OnRequestPermissionsResult(int requestCode, string[] permissions, [GeneratedEnum] Android.Content.PM.Permission[] grantResults)
        {
            Xamarin.Essentials.Platform.OnRequestPermissionsResult(requestCode, permissions, grantResults);

            base.OnRequestPermissionsResult(requestCode, permissions, grantResults);
        }
    }
}