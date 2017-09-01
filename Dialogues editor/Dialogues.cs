using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Dialogues_editor
{
    [Serializable]
    public class Dialogues_Collection
    {
        /////////////////////////////////////////////////////////
        //  Variables
        /////////////////////////////////////////////////////////

        public SortedDictionary<string, Dialogue> dialogues
        {
            get
            {
                return this.dialogues_;
            }

            set
            {
                if (value != null && value.Any())
                {
                    foreach (var item in value)
                    {
                        this.dialogues_.Add(item.Key, new Dialogue(item.Value));
                    }
                }
            }


        }
        protected SortedDictionary<string, Dialogue> dialogues_ = new SortedDictionary<string, Dialogue>();




        /////////////////////////////////////////////////////////
        //  Constructors
        /////////////////////////////////////////////////////////

        public Dialogues_Collection()
        {

            try
            {
            }
            catch (Exception) { }

        }

        public Dialogues_Collection(Dialogues_Collection dialogues_collection)
        {
            this.dialogues = dialogues_collection.dialogues;
        }




        /////////////////////////////////////////////////////////
        //  Methods
        /////////////////////////////////////////////////////////

        public bool validate_id(string id)
        {
            return (id != null
                && this.dialogues_.ContainsKey(id)
                && this.dialogues_[id].id != null);
        }


        /// <summary>
        /// Adds dialogue to dialogues list.
        /// </summary>
        public bool add(Dialogue dialogue)
        {
            // Validate input data.
            if (dialogue == null)
                throw new Exception("Dialogue is null, cannot add to SortedDictionary.");

            if (dialogue.id == null)
                throw new Exception("Dialogue ID is null, cannot add to SortedDictionary.");

            if (this.dialogues.Any(item => item.Key.Equals(dialogue.id)))
                throw new Exception("Dialogue ID is already in the list.");


            // Add dialogue.
            this.dialogues.Add(dialogue.id, dialogue);

            // Adds leads_to references to ther dialogues.
            foreach (var caused in dialogue.caused_by)
            {
                //Console.WriteLine(caused + " leads to: " + dialogue.id);

                if (this.dialogues_.ContainsKey(caused))
                {
                    var found = this.dialogues[caused];

                    if (!found.leads_to.Any(item => item.Equals(dialogue.id)))
                        found.leads_to.Add(dialogue.id);
                }
            }

            // Adds caused_by references to ther dialogues.
            foreach (var leads in dialogue.leads_to)
            {
                //Console.WriteLine(leads + " caused by: " + dialogue.id);

                if (this.dialogues_.ContainsKey(leads))
                {
                    var found = this.dialogues[leads];
                    if (!found.caused_by.Any(item => item.Equals(dialogue.id)))
                        found.caused_by.Add(dialogue.id);
                }
            }

            return true;
        }


        /// <summary>
        /// Removes dialogue from dialogues list.
        /// </summary>
        public void remove(string dialogue_id)
        {
            // Validate input data.
            if (!this.validate_id(dialogue_id))
                throw new Exception("Cannot remove dialogue with ID: " + dialogue_id + " from SortedDictionary.");


            // Reference to selected dialogue.
            var dialogue = this.dialogues[dialogue_id];

            // Remove all leads_to this dialogue id reference.
            foreach (var caused in dialogue.caused_by)
            {
                if (this.dialogues_.ContainsKey(caused))
                {
                    var found = this.dialogues[caused];
                    found.leads_to.Remove(dialogue_id);
                }
            }

            // Remove all caused_by to this dialogue id rederence.
            foreach (var leads in dialogue.leads_to)
            {
                if (this.dialogues_.ContainsKey(leads))
                {
                    var found = this.dialogues[leads];
                    found.caused_by.Remove(dialogue_id);
                }
            }

            // Finaly remove this dialogue from list.
            this.dialogues.Remove(dialogue_id);
        }


        /// <summary>
        /// Generetas an unique ID.
        /// </summary>
        public string generate_unique_id()
        {
            string new_id = null;
            bool found = false;

            while (new_id == null || found == true)
            {
                new_id = "ID_" + DateTime.UtcNow.Ticks;
                found = this.dialogues.Any(item => item.Key.Equals(new_id));
            }

            return new_id;
        }


        /// <summary>
        /// Changes ID of dialogue.
        /// </summary>
        public bool change_id(string dialogue_id, string new_id)
        {
            if (!this.validate_id(dialogue_id))
                return false;

            if (this.validate_id(new_id))
                return false;

            string new_ID = Formatter.get_formatted_or_null_id(new_id);
            if (new_ID == null)
                return false;

            // Reference to selected dialogue.
            var dialogue_ref = this.dialogues[dialogue_id];



            // Removing current one.
            this.dialogues_.Remove(dialogue_id);


            // Change leads_to references to dialogue.
            foreach (var caused in dialogue_ref.caused_by)
            {
                //Console.WriteLine(caused + " leads to: " + dialogue_id);

                if (this.dialogues_.ContainsKey(caused))
                {
                    var found = this.dialogues[caused];

                    //var element = found.leads_to.FirstOrDefault(x => x.Equals(dialogue_id));
                    //element = new_ID;

                    int index = 0;
                    bool index_found = false;

                    foreach (var item in found.leads_to)
                    {
                        //Console.WriteLine(item + " compared with: " + dialogue_id);
                        if (item.Equals(dialogue_id))
                        {
                            index_found = true;
                            break;
                        }
                        index++;
                    }

                    if (index_found)
                    {
                        found.leads_to[index] = new_ID;
                    }
                }
            }

            // Change caused_by references to dialogue.
            foreach (var leads in dialogue_ref.leads_to)
            {
                //Console.WriteLine(leads + " caused by: " + dialogue_id);

                if (this.dialogues_.ContainsKey(leads))
                {
                    var found = this.dialogues[leads];

                    //var element = found.caused_by.FirstOrDefault(x => x.Equals(dialogue_id));
                    //element = new_ID;


                    int index = 0;
                    bool index_found = false;

                    foreach (var item in found.caused_by)
                    {
                        //Console.WriteLine(item + " compared with: " + dialogue_id);
                        if (item.Equals(dialogue_id))
                        {
                            index_found = true;
                            break;
                        }
                        index++;
                    }

                    if (index_found)
                    {
                        found.caused_by[index] = new_ID;
                    }
                }
            }


            // Assign new id.
            dialogue_ref.id = new_ID;

            // Adding it again.
            this.dialogues_.Add(dialogue_ref.id, dialogue_ref);


            return true;
        }


        public bool create_link(string root, string leads_to)
        {
            if (!this.validate_id(root))
                throw new Exception("Cannot create link ID: " + root + " does not exist in SortedDictionary.");

            if (!this.validate_id(leads_to))
                throw new Exception("Cannot create link ID: " + root + " does not exist in SortedDictionary.");

            Dialogue root_ref = this.dialogues_[root];
            Dialogue leads_to_ref = this.dialogues_[leads_to];

            if (root_ref.id.Equals(leads_to_ref.id))
                return false;

            if (!root_ref.leads_to.Any(item => item.Equals(leads_to)))
                root_ref.leads_to.Add(leads_to);

            if (!leads_to_ref.caused_by.Any(item => item.Equals(root)))
                leads_to_ref.caused_by.Add(root);

            return true;
        }
    }

    [Serializable]
    public enum Condition_result : short
    {
        NONE,
        INVISIBLE,
        INELIGIBLE
    }


    [Serializable]
    public class Dialogue
    {
        /////////////////////////////////////////////////////////
        //  Variables
        /////////////////////////////////////////////////////////

        /// <summary>
        /// ID of the dialogue.
        /// </summary>
        public string id
        {
            get
            {
                return this.id_;
            }

            set
            {
                if (value != null)
                    this.id_ = value.ToUpper();
                else
                    this.id_ = null;
            }
        }
        protected string id_ = null;


        /// <summary>
        /// Person ID of the dialogue.
        /// </summary>
        public string person_id
        {
            get
            {
                return this.person_id_;
            }

            set
            {
                if (value != null)
                    this.person_id_ = value.ToUpper();
                else
                    this.person_id_ = null;
            }
        }
        protected string person_id_ = null;


        /// <summary>
        /// Represents value if translation has been approved by Bognerkie.
        /// </summary>
        public bool translation_approved
        {
            get
            {
                return this.translation_approved_;
            }

            set
            {
                this.translation_approved_ = value;
            }
        }
        protected bool translation_approved_ = false;


        /// <summary>
        /// Polish version of short dialogue description.
        /// </summary>
        public string pl
        {
            get
            {
                return this.pl_;
            }

            set
            {
                if (value != null)
                    this.pl_ = string.Copy(value);
                else
                    this.pl_ = null;
            }
        }
        protected string pl_ = null;

        /// <summary>
        /// English version of short dialogue description.
        /// </summary>
        public string en
        {
            get
            {
                return this.en_;
            }

            set
            {
                if (value != null)
                    this.en_ = string.Copy(value);
                else
                    this.en_ = null;
            }
        }
        protected string en_ = null;

        /// <summary>
        /// German version of short dialogue description.
        /// </summary>
        public string de
        {
            get
            {
                return this.de_;
            }

            set
            {
                if (value != null)
                    this.de_ = string.Copy(value);
                else
                    this.de_ = null;
            }
        }
        protected string de_ = null;


        /// <summary>
        /// List of IDs that this dialogue is caused by.
        /// </summary>
        public List<string> caused_by
        {
            get
            {
                return this.caused_by_;
            }

            set
            {
                this.caused_by_ = value;
            }
        }
        protected List<string> caused_by_ = new List<string>();

        /// <summary>
        /// List of IDs that this dialogue leads to.
        /// </summary>
        public List<string> leads_to
        {
            get
            {
                return this.leads_to_;
            }

            set
            {
                this.leads_to_ = value;
            }
        }
        protected List<string> leads_to_ = new List<string>();


        public bool auto_choose
        {
            get
            {
                return this.auto_choose_;
            }

            set
            {
                this.auto_choose_ = value;
                if (this.auto_choose_ == true)
                    this.condition_result_ = Condition_result.NONE;
            }
        }
        protected bool auto_choose_ = false;

        public bool auto_continue
        {
            get
            {
                return this.auto_continue_;
            }

            set
            {
                this.auto_continue_ = value;
            }
        }
        protected bool auto_continue_ = true;


        public bool is_conditional
        {
            get
            {
                return this.is_conditional_;
            }

            set
            {
                this.is_conditional_ = value;
                if (!this.is_conditional_)
                {
                    this.condition_result_ = Condition_result.NONE;
                    this.condition_id_ = null;
                }

            }
        }
        protected bool is_conditional_ = false;

        public string condition_id
        {
            get
            {
                return this.condition_id_;
            }

            set
            {
                if (value != null)
                    this.condition_id_ = value.ToUpper();
                else
                    this.condition_id_ = null;
            }
        }
        protected string condition_id_ = null;

        public Condition_result condition_result
        {
            get
            {
                return this.condition_result_;
            }

            set
            {
                this.condition_result_ = value;
            }
        }
        protected Condition_result condition_result_ = Condition_result.NONE;


        public bool runs_script
        {
            get
            {
                return this.runs_script_;
            }

            set
            {
                this.runs_script_ = value;
            }
        }
        protected bool runs_script_ = false;

        public string runs_script_id
        {
            get
            {
                return this.runs_script_id_;
            }

            set
            {
                if (value != null)
                    this.runs_script_id_ = string.Copy(value);
                else
                    this.runs_script_id_ = null;
            }
        }
        protected string runs_script_id_ = null;




        /////////////////////////////////////////////////////////
        //  Constructors
        /////////////////////////////////////////////////////////

        public Dialogue()
        {
        }

        public Dialogue(Dialogue dialogue)
        {
            this.id = dialogue.id;

            this.person_id = dialogue.person_id;

            this.translation_approved = dialogue.translation_approved;

            this.pl = dialogue.pl;
            this.en = dialogue.en;
            this.de = dialogue.de;

            foreach (var item in dialogue.caused_by)
            {
                this.caused_by.Add(string.Copy(item));
            }

            foreach (var item in dialogue.leads_to)
            {
                this.leads_to.Add(string.Copy(item));
            }

            this.auto_choose = dialogue.auto_choose;
            this.auto_continue = dialogue.auto_continue;

            this.is_conditional = dialogue.is_conditional;
            this.condition_id = dialogue.condition_id;
            this.condition_result = dialogue.condition_result;

            this.runs_script = dialogue.runs_script;
            this.runs_script_id = dialogue.runs_script_id;
        }




        /////////////////////////////////////////////////////////
        //  Methods
        /////////////////////////////////////////////////////////

        public override string ToString()
        {
            return this.id;
        }


        public bool swap_leads_to(int id, int with_id)
        {
            if (id >= 0 && id < this.leads_to_.Count
                && with_id >= 0 && with_id < this.leads_to_.Count)
            {
                var temp_ref = this.leads_to_[id];

                this.leads_to_[id] = this.leads_to_[with_id];
                this.leads_to_[with_id] = temp_ref;

                return true;
            }
            else
            {
                return false;
            }
        }
    }
}
