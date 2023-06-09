using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using Firebase.Firestore;
using Firebase;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using AndroidX.RecyclerView.Widget;
using Bonganciso_MyApp_FlashBox.DataModel;
using Bonganciso_MyApp_FlashBox.Adapter;
using Android.Gms.Tasks;
using static Bonganciso_MyApp_FlashBox.Adapter.StudyAdapter;
using static Android.Icu.Text.CaseMap;
using System.Security.Cryptography;

namespace Bonganciso_MyApp_FlashBox
{
    [Activity(Label = "Homepage")]
    public class Homepage : Activity, IOnSuccessListener
    {
        private RecyclerView recyclerView;
        private FirebaseFirestore db;
        private List<Study> studyList;
        private StudyAdapter studyAdapter;
        string username;
        EditText boxTitle, boxDescription, boxCategory;
        string title, desc, cat, front, back;
        EditText cardFront, cardBack;
        EditText editTitle, editDesc, editCat;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);
            Xamarin.Essentials.Platform.Init(this, savedInstanceState);
            // Set our view from the "main" layout resource
            SetContentView(Resource.Layout.homepage);

            username = Intent.GetStringExtra("Username");

            FirebaseApp.InitializeApp(this);
            // Get the Firestore instance
            db = FirebaseFirestore.Instance;

            recyclerView = FindViewById<RecyclerView>(Resource.Id.recyclerview);
            recyclerView.SetLayoutManager(new LinearLayoutManager(this));

            studyList = new List<Study>();
            studyAdapter = new StudyAdapter(studyList);
            studyAdapter.ItemClick += StudyAdapter_ItemClick;
            recyclerView.SetAdapter(studyAdapter);

            FetchData();

            ImageButton addcat = FindViewById<ImageButton>(Resource.Id.btnaddcategory);
            ImageButton addstudy = FindViewById<ImageButton>(Resource.Id.btnaddstudy); //just add a box
            ImageButton logout = FindViewById<ImageButton>(Resource.Id.logout);
            logout.Click += Logout_Click;
            
            addcat.Click += Addcat_Click;
            addstudy.Click += Addstudy_Click;
        }

        private void Logout_Click(object sender, EventArgs e)
        {
            Intent intent = new Intent(this, typeof(MainActivity));
            StartActivity(intent);
        }

        private void StudyAdapter_ItemClick(object sender, StudyAdapterClickEventArgs e)
        {
            Study clickedStudy = studyList[e.Position];
            string a = clickedStudy.studyID;
            string b = clickedStudy.title;
            string c = clickedStudy.description;
            string d = clickedStudy.category;

            DisplayStudy(a, b, c, d);

        }


        public void FetchData()
        {
            // Clear the studyList before fetching data
            studyList.Clear();
            studyAdapter.NotifyDataSetChanged();

            CollectionReference studyBoxRef = db.Collection("User").Document(username).Collection("StudyBox");

            studyBoxRef.Get().AddOnSuccessListener(new FetchDataSuccessListener(studyList, studyAdapter));
        }
        public class OnCompleteListener : Java.Lang.Object, IOnCompleteListener
        {
            private string username;
            private FirebaseFirestore db;
            private ImageView imageView;
            private List<Study> studyList;
            private StudyAdapter studyAdapter;

            public OnCompleteListener(string username, FirebaseFirestore db, ImageView imageView, List<Study> studyList, StudyAdapter studyAdapter)
            {
                this.username = username;
                this.db = db;
                this.imageView = imageView;
                this.studyList = studyList;
                this.studyAdapter = studyAdapter;
            }

            public void OnComplete(Task task)
            {
                if (task.IsSuccessful)
                {
                    DocumentSnapshot document = (DocumentSnapshot)task.Result;
                    if (document.Exists() && document.Contains("StudyBox"))
                    {
                        imageView.Visibility = ViewStates.Gone;
                        FetchStudyData();
                    }
                    else
                    {
                        // Handle the case when the "StudyBox" collection doesn't exist
                        imageView.Visibility = ViewStates.Visible;
                    }
                }
                else
                {
                    // Handle any errors
                }
            }
            private void FetchStudyData()
            {
                studyList.Clear();
                studyAdapter.NotifyDataSetChanged();
                CollectionReference studyBoxRef = db.Collection("User").Document(username).Collection("StudyBox");

                studyBoxRef.Get().AddOnSuccessListener(new FetchDataSuccessListener(studyList, studyAdapter));

            }
        }
        public class FetchDataSuccessListener : Java.Lang.Object, IOnSuccessListener
        {
            private List<Study> studyList;
            private StudyAdapter studyAdapter;

            public FetchDataSuccessListener(List<Study> studyList, StudyAdapter studyAdapter)
            {
                this.studyList = studyList;
                this.studyAdapter = studyAdapter;
            }

            public void OnSuccess(Java.Lang.Object result)
            {
                var snapshot = (QuerySnapshot)result;
                if (!snapshot.IsEmpty)
                {
                    var documents = snapshot.Documents;
                    foreach (DocumentSnapshot document in documents)
                    {
                        Study study = new Study
                        {
                            studyID = document.Id,
                            title = document.GetString("Title") ?? "",
                            description = document.GetString("Description") ?? "",
                            category = document.GetString("Category") ?? ""
                        };

                        studyList.Add(study);
                    }

                    studyAdapter.NotifyDataSetChanged();
                }
            }
        }
        public void OnSuccess(Java.Lang.Object result)
        {
            var snapshot = (QuerySnapshot)result;
            if (!snapshot.IsEmpty)
            {
                studyList.Clear(); // Clear the existing list before adding new studies
                var documents = snapshot.Documents;
                foreach (DocumentSnapshot document in documents)
                {
                    Study study = new Study
                    {
                        title = document.GetString("Title") ?? "",
                        description = document.GetString("Description") ?? "",
                        category = document.GetString("Category") ?? ""
                    };
                    studyList.Add(study);
                }
                studyAdapter.NotifyDataSetChanged();
            }
        }



        private void DisplayStudy(string a, string b, string c, string d)
        {
            AlertDialog.Builder dialogBuilder = new AlertDialog.Builder(this);
            LayoutInflater inflater = LayoutInflater.From(this);

            // Inflate the custom dialog layout
            View dialogView = inflater.Inflate(Resource.Layout.editstudy, null);
            dialogBuilder.SetView(dialogView);

            // Get the EditText view from the dialog layout
            editTitle = dialogView.FindViewById<EditText>(Resource.Id.editTitle);
            editDesc = dialogView.FindViewById<EditText>(Resource.Id.editDesc);
            editCat = dialogView.FindViewById<EditText>(Resource.Id.editCategory);
            //display the data
            editTitle.Text = b;
            editDesc.Text = c;
            editCat.Text = d;
           
            dialogBuilder.SetPositiveButton("Update", (dialog, which) =>
            {
                //edit
                string edit1 = editTitle.Text, edit2 = editDesc.Text, edit3 = editCat.Text;
                bool validate = ValidateField(edit1, edit2, edit2);
                if(validate == true)
                {
                    UpdateStudy(a, edit1, edit2, edit3);
                    // Update the EditText fields with the new values
                    editTitle.Text = edit1;
                    editDesc.Text = edit2;
                    editCat.Text = edit3;
                    Toast.MakeText(this, "Successfully updated", ToastLength.Short).Show();
                    FetchData();
                }
                
                
                
            });
            dialogBuilder.SetNegativeButton("Delete", (dialog, which) =>
            {
                AlertDialog.Builder innerBuilder = new AlertDialog.Builder(this);
                innerBuilder.SetTitle("Confirmation");
                innerBuilder.SetMessage("Are you sure you want to delete?");

                innerBuilder.SetPositiveButton("Yes", (innerDialog, innerWhich) =>
                {
                    // Perform delete action
                    DocumentReference docRef = db.Collection("User").Document(username).Collection("StudyBox").Document(a);
                    docRef.Delete();
                    Toast.MakeText(this, "Successfully deleted", ToastLength.Short).Show();
                    FetchData();

                    // Add any additional actions after deletion
                });

                innerBuilder.SetNegativeButton("No", (innerDialog, innerWhich) =>
                {
                    // Inner AlertDialog canceled, continue with the outer AlertDialog
                    AlertDialog alertDialog = (AlertDialog)innerDialog;
                    alertDialog.Dismiss();
                });

                AlertDialog innerDialog = innerBuilder.Create();
                innerDialog.Show();

            });

            dialogBuilder.SetNeutralButton("Cancel", (dialog, which) =>
            {
                //cancel
                AlertDialog alertDialog = (AlertDialog)dialog;
                alertDialog.Dismiss();
            });

            // Create and show the dialog
            AlertDialog alertDialog = dialogBuilder.Create();
            alertDialog.Show();

        }
        private void UpdateStudy(string a, string b, string c, string d)
        {
            DocumentReference studyRef = db.Collection("User").Document(username).Collection("StudyBox").Document(a);

            var data = new JavaDictionary<string, Java.Lang.Object>
            {
                { "Title", new Java.Lang.String(b) },
                { "Description", new Java.Lang.String(c) },
                { "Category", new Java.Lang.String(d) },
                { "Status", "Active" }
            };

            studyRef.Set(data);
        }

        //add category, study, and cards here
        private void Addstudy_Click(object sender, EventArgs e)
        {
            DisplayAddStudy();
        }
        private void Addcat_Click(object sender, EventArgs e)
        {
            DisplayAddCategory();
        }
        private void DisplayAddStudy()
        {
            //display add_box
            Dialog dialog = new Dialog(this);

            // Set the custom layout for the dialog
            dialog.SetContentView(Resource.Layout.add_study);

            // Set the dialog to occupy the full screen
            dialog.Window.SetLayout(WindowManagerLayoutParams.MatchParent, WindowManagerLayoutParams.MatchParent);

            boxTitle = dialog.FindViewById<EditText>(Resource.Id.textTitle);
            boxDescription = dialog.FindViewById<EditText>(Resource.Id.textDesc);
            boxCategory = dialog.FindViewById<EditText>(Resource.Id.textCategory);

            title = boxTitle.Text;
            desc = boxDescription.Text;
            cat = boxCategory.Text;

            Button addbox = dialog.FindViewById<Button>(Resource.Id.btnaddbox);
            Button addboxcard = dialog.FindViewById<Button>(Resource.Id.btnaddboxcard);
            ImageButton back = dialog.FindViewById<ImageButton>(Resource.Id.backbtnstudy);
            back.Click += (sender, e) =>
            {
                EmptyField(boxTitle, boxDescription, boxCategory);
                dialog.Dismiss();
            };
            addbox.Click += (sender, e) =>
            {
                title = boxTitle.Text;
                desc = boxDescription.Text;
                cat = boxCategory.Text;
                bool isValid = ValidateField(title, desc, cat); //check if all field is empty
                                                                //bool exists = ValidateUsername(username);
                if (isValid == true)
                {
                    SaveBox(title, desc, cat);
                    Toast.MakeText(this, "Study has been added", ToastLength.Short).Show();
                    EmptyField(boxTitle, boxDescription, boxCategory);
                    dialog.Dismiss();
                    
                }
            };
            addboxcard.Click += (sender, e) =>
            {
                title = boxTitle.Text;
                desc = boxDescription.Text;
                cat = boxCategory.Text;
                bool isValid = ValidateField(title, desc, cat); //check if all field is empty
                                                                //bool exists = ValidateUsername(username);
                if (isValid == true)
                {
                    SaveBox(title, desc, cat);
                    Toast.MakeText(this, "Study has been added", ToastLength.Short).Show();
                    EmptyField(boxTitle, boxDescription, boxCategory);
                    dialog.Dismiss();
                    DisplayCard();
                }
            };
            dialog.Show();

        }
        private void SaveBox(string title, string desc, string cat)
        {

            DocumentReference docRef = db.Collection("User").Document(username).Collection("StudyBox").Document();

            // Create a data object to save
            var study = new JavaDictionary<string, Java.Lang.Object>
                {
                     { "Title", new Java.Lang.String(title) },
                     { "Description", new Java.Lang.String(desc) },
                     { "Category", new Java.Lang.String(cat) },
                     { "Status", new Java.Lang.String("Active") },

                };

            // Save the document to Firestore
            docRef.Set(study);


            // Display a toast message
            Toast.MakeText(this, "Study added", ToastLength.Short).Show();
            EmptyField(boxTitle, boxDescription, boxCategory);

            FetchData();
        }
        private void DisplayCard()
        {
            //display add_card
            Dialog dialog = new Dialog(this);

            // Set the custom layout for the dialog
            dialog.SetContentView(Resource.Layout.add_card);

            // Set the dialog to occupy the full screen
            dialog.Window.SetLayout(WindowManagerLayoutParams.MatchParent, WindowManagerLayoutParams.MatchParent);

            cardFront = dialog.FindViewById<EditText>(Resource.Id.textfront);
            cardBack = dialog.FindViewById<EditText>(Resource.Id.textback);

            Button addcard = dialog.FindViewById<Button>(Resource.Id.btncard);
            Button addmore = dialog.FindViewById<Button>(Resource.Id.btncardmore);
            ImageButton backcard = dialog.FindViewById<ImageButton>(Resource.Id.backbtncard);
            backcard.Click += (sender, e) =>
            {
                EmptyField(cardFront, cardBack);
                dialog.Dismiss();
            };
            addcard.Click += (sender, e) =>
            {
                front = cardFront.Text;
                back = cardBack.Text;
                bool isValid = ValidateField(front, back); //check if all field is empty
                                                           //bool exists = ValidateUsername(username);
                if (isValid == true)
                {
                    SaveCard(front, back);
                    Toast.MakeText(this, "Card has been added", ToastLength.Short).Show();
                    EmptyField(cardFront, cardBack);
                    dialog.Dismiss();
                    
                }
            };
            addmore.Click += (sender, e) =>
            {
                front = cardFront.Text;
                back = cardBack.Text;
                bool isValid = ValidateField(front, back); //check if all field is empty
                                                           //bool exists = ValidateUsername(username);
                if (isValid == true)
                {
                    SaveCard(front, back);
                    Toast.MakeText(this, "Card has been added.", ToastLength.Short).Show();
                    EmptyField(cardFront, cardBack);
                    DisplayCard();
                }
            };
            dialog.Show();
        }
        private void SaveCard(string front, string back)
        {

            DocumentReference docRef = db.Collection("User").Document(username).Collection("StudyBox").Document(title).Collection("StudyCard").Document();

            // Create a data object to save
            var data = new JavaDictionary<string, Java.Lang.Object>
                                {
                                    { "Front", new Java.Lang.String(front) },
                                    { "Back", new Java.Lang.String(back) },

                                 };

            // Save the document to Firestore
            docRef.Set(data);


            // Display a toast message
            Toast.MakeText(this, "Card added", ToastLength.Short).Show();
            EmptyField(cardFront, cardBack);

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
        public void EmptyField(params EditText[] fields)
        {
            // Iterate through each field and check if it's not empty
            foreach (EditText field in fields)
            {
                field.Text = string.Empty;
            }

        }
        // ******** add category
        private void DisplayAddCategory()
        {
            
                AlertDialog.Builder dialogBuilder = new AlertDialog.Builder(this);
                LayoutInflater inflater = LayoutInflater.From(this);

                // Inflate the custom dialog layout
                View dialogView = inflater.Inflate(Resource.Layout.add_category, null);
                dialogBuilder.SetView(dialogView);

                // Get the EditText view from the dialog layout
                EditText getcat = dialogView.FindViewById<EditText>(Resource.Id.textgetcategory);

            // Set the negative button click listener
            

            // Set the positive button click listener
            dialogBuilder.SetPositiveButton("Save", (dialog, which) =>
            {
                String cat = getcat.Text;
                if (string.IsNullOrEmpty(cat))
                {
                    // Display a toast message
                    Toast.MakeText(this, "Fill in the field", ToastLength.Short).Show();
                }
                else
                {
                    SaveDBCategory(cat);
                    getcat.Text = string.Empty;
                    DisplayAddCategory();

                }

            });
            dialogBuilder.SetNegativeButton("Cancel", (dialog, which) =>
            {
                getcat.Text = string.Empty;
            });
            dialogBuilder.SetNeutralButton("Add Another", (dialog, which) =>
            {
                String cat = getcat.Text;
                if (string.IsNullOrEmpty(cat))
                {
                    // Display a toast message
                    Toast.MakeText(this, "Fill in the field", ToastLength.Short).Show();
                }
                else
                {
                    SaveDBCategory(cat);
                    getcat.Text = string.Empty;
                    DisplayAddCategory();

                }
            });

            // Create and show the dialog
            AlertDialog alertDialog = dialogBuilder.Create();
            alertDialog.Show();
            
        }
        private void SaveDBCategory(String category)
        {
            // Create a new document with a generated ID
            DocumentReference docRef = db.Collection("User").Document(username).Collection("Category").Document();

            // Create a data object to save
            var data = new JavaDictionary<string, Java.Lang.Object>
            {
                { "CategoryName", new Java.Lang.String(category) },

            };

            // Save the document to Firestore
            docRef.Set(data);


            // Display a toast message
            Toast.MakeText(this, "" + category + " added", ToastLength.Short).Show();
        }
        // ****** end add category

    }
}
