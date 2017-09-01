using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

using System.Web.Script.Serialization;
using System.IO;

namespace Dialogues_editor
{
    public partial class Form1 : Form
    {
        /////////////////////////////////////////////////////////
        //  Const values
        /////////////////////////////////////////////////////////

        protected const int max_recently_opened_ = 9;



        /////////////////////////////////////////////////////////
        //  Variables
        /////////////////////////////////////////////////////////

        protected string window_title_ = "Dialogue editor v1.0";

        protected string file_path_ { get; set; } = null;
        protected string file_path_name_ { get; set; } = null;
        protected bool file_can_save_ { get; set; } = false;

        protected Dialogues_Collection dialogues_ = new Dialogues_Collection();

        protected bool changed_ { get; set; } = false;

        protected string current_selected_left_id_ { get; set; } = null;
        protected string current_selected_main_id_ { get; set; } = null;
        protected string current_selected_right_id_ { get; set; } = null;


        // Connected with menu:

        protected bool editing_ { get; set; } = true;




        /////////////////////////////////////////////////////////
        //  Constructors
        /////////////////////////////////////////////////////////

        public Form1()
        {
            InitializeComponent();

            //var a = new Dialogue() { id = "abc", en = "O damn.", pl = "O kucze.", leads_to = new List<string> { "DAMN", "ID1", "123465" } };
            //var b = new Dialogue() { id = "damn", en = "Aasd.", pl = "Ziemniak.", caused_by = new List<string> { "ABC" } };
            //var c = new Dialogue() { id = "id1", en = "Nothinh.", caused_by = new List<string> { "ABC" } };
            //var d = new Dialogue() { id = "123465", en = "NotAAShinh.654", caused_by = new List<string> { "ABC" } };

            //this.dialogues_.add(a);
            //this.dialogues_.add(b);
            //this.dialogues_.add(c);
            //this.dialogues_.add(d);
            //this.dialogues_.remove(d.id);
            //this.dialogues_.change_id(a.id, "TAK");




            //string json_resoult = (new JavaScriptSerializer()).Serialize(this.dialogues_);

            //Console.WriteLine(json_resoult);


            // Initializing Menus.
            this.update_menu_state();
            this.update_main_panel(null);
            this.update_main_search_list(null);

            //this.update_main_panel(c.id);
            //this.update_main_search_list(c.id);

        }




        /////////////////////////////////////////////////////////
        //  Methods
        /////////////////////////////////////////////////////////

        protected void update_menu_state()
        {
            // Enabling or disabling buttons.
            this.saveToolStripMenuItem.Enabled = this.file_can_save_;


            // Label for window.
            string whole_label = string.Copy(this.window_title_);
            if (this.file_path_name_ != null)
                whole_label += " [" + this.file_path_name_ + "]";

            if (this.changed_ == true)
                whole_label += " [Unsaved changes]";

            this.Text = whole_label;
        }


        protected void update_main_panel(string id)
        {
            // Validating id.
            bool exist = this.dialogues_.validate_id(id);
            Dialogue dialogue_ref = null;

            // Saving value.
            this.current_selected_main_id_ = exist ? string.Copy(id) : null;

            // Filling up with data.
            if (exist)
            {
                dialogue_ref = this.dialogues_.dialogues[id];

                this.checkBox_m_choose.Checked = dialogue_ref.auto_choose;
                this.checkBox_m_conditional.Checked = dialogue_ref.is_conditional;
                this.checkBox_m_runs_script.Checked = dialogue_ref.runs_script;
                this.checkBox_m_translation.Checked = dialogue_ref.translation_approved;
                this.checkBox_m_a_continue.Checked = dialogue_ref.auto_continue && dialogue_ref.auto_choose;

                this.textBox_m_condition_id.Text = dialogue_ref.condition_id;
                this.textBox_m_id_person.Text = dialogue_ref.person_id;
                this.textBox_m_id_selected.Text = dialogue_ref.id;
                this.textBox_m_runs_script_id.Text = dialogue_ref.runs_script_id;

                this.richTextBox_m_de.Text = dialogue_ref.de;
                this.richTextBox_m_en.Text = dialogue_ref.en;
                this.richTextBox_m_pl.Text = dialogue_ref.pl;

                this.comboBox_m_condition_result.SelectedIndex = (short)dialogue_ref.condition_result;
            }
            else
            {
                this.checkBox_m_choose.Checked = false;
                this.checkBox_m_conditional.Checked = false;
                this.checkBox_m_runs_script.Checked = false;
                this.checkBox_m_translation.Checked = false;
                this.checkBox_m_a_continue.Checked = false;

                this.textBox_m_condition_id.Text = null;
                this.textBox_m_id_person.Text = null;
                this.textBox_m_id_selected.Text = null;
                this.textBox_m_runs_script_id.Text = null;

                this.richTextBox_m_de.Text = null;
                this.richTextBox_m_en.Text = null;
                this.richTextBox_m_pl.Text = null;

                this.comboBox_m_condition_result.SelectedIndex = (short)Condition_result.NONE;

                this.listBox_m_caused_by.Items.Clear();
            }






            // Enabling or disabling.
            this.checkBox_m_choose.Enabled = editing_ && exist;
            this.checkBox_m_conditional.Enabled = editing_ && exist;
            this.checkBox_m_runs_script.Enabled = editing_ && exist;
            this.checkBox_m_translation.Enabled = !editing_ && exist;
            this.checkBox_m_a_continue.Enabled = editing_ && exist && this.checkBox_m_choose.Checked;

            this.button_m_caused_by_remove.Enabled = editing_ && exist;
            this.button_m_delete.Enabled = editing_ && exist;
            this.button_m_id_change.Enabled = editing_ && exist;
            this.button_m_leads_to_create.Enabled = editing_ && exist;
            this.button_m_leads_to_down.Enabled = editing_ && exist;
            this.button_m_leads_to_up.Enabled = editing_ && exist;
            this.button_m_leads_to_remove.Enabled = editing_ && exist;
            this.button_m_pure_new.Enabled = editing_;

            this.textBox_m_id_person.Enabled = editing_ && exist;
            this.textBox_m_id_selected.Enabled = editing_ && exist;
            this.textBox_m_condition_id.Enabled = editing_ && exist && this.checkBox_m_conditional.Checked;

            this.richTextBox_m_de.Enabled = exist;
            this.richTextBox_m_en.Enabled = exist;
            this.richTextBox_m_pl.Enabled = exist;

            this.comboBox_m_condition_result.Enabled = editing_ && exist
                && this.checkBox_m_conditional.Checked && !this.checkBox_m_choose.Checked;

            this.textBox_m_runs_script_id.Enabled = editing_ && exist && this.checkBox_m_runs_script.Checked;

            Console.WriteLine(this.current_selected_main_id_);

            // Update lists.
            this.update_caused_by_leads_to_lists(id);

            // Try update left and right panels.
            if (this.listBox_m_leads_to.Items.Count > 0 &&
                (this.listBox_m_leads_to.SelectedIndex == -1 || this.listBox_m_leads_to.Items[0].ToString().Equals(this.current_selected_right_id_)))
            {
                this.update_right_panel(this.listBox_m_leads_to.Items[0].ToString());
                this.update_right_search_list(this.current_selected_right_id_);
            }
            else
            {
                this.update_right_panel(null);
                this.update_right_search_list(null);
            }


            if (this.listBox_m_caused_by.Items.Count > 0 &&
                (this.listBox_m_caused_by.SelectedIndex == -1 || this.listBox_m_caused_by.Items[0].ToString().Equals(this.current_selected_left_id_)))
            {
                this.update_left_panel(this.listBox_m_caused_by.Items[0].ToString());
                this.update_left_search_list(this.current_selected_left_id_);
            }
            else
            {
                this.update_left_panel(null);
                this.update_left_search_list(null);
            }
        }


        protected void update_right_panel(string id)
        {
            // Validating id.
            bool exist = this.dialogues_.validate_id(id);
            Dialogue dialogue_ref = null;

            // Saving value.
            this.current_selected_right_id_ = exist ? string.Copy(id) : null;
            this.select_back_caused_by_leads_to();


            // Filling up with data.
            if (exist)
            {
                dialogue_ref = this.dialogues_.dialogues[id];

                this.checkBox_r_choose.Checked = dialogue_ref.auto_choose;
                this.checkBox_r_conditional.Checked = dialogue_ref.is_conditional;
                this.checkBox_r_runs_script.Checked = dialogue_ref.runs_script;
                this.checkBox_r_translation.Checked = dialogue_ref.translation_approved;
                this.checkBox_r_a_continue.Checked = dialogue_ref.auto_continue && dialogue_ref.auto_choose;

                this.textBox_r_condition_id.Text = dialogue_ref.condition_id;
                this.textBox_r_id_person.Text = dialogue_ref.person_id;
                this.textBox_r_id_selected.Text = dialogue_ref.id;
                this.textBox_r_runs_script_id.Text = dialogue_ref.runs_script_id;

                this.richTextBox_r_de.Text = dialogue_ref.de;
                this.richTextBox_r_en.Text = dialogue_ref.en;
                this.richTextBox_r_pl.Text = dialogue_ref.pl;

                this.comboBox_r_condition_result.SelectedIndex = (short)dialogue_ref.condition_result;
            }
            else
            {
                this.checkBox_r_choose.Checked = false;
                this.checkBox_r_conditional.Checked = false;
                this.checkBox_r_runs_script.Checked = false;
                this.checkBox_r_translation.Checked = false;
                this.checkBox_r_a_continue.Checked = false;

                this.textBox_r_condition_id.Text = null;
                this.textBox_r_id_person.Text = null;
                this.textBox_r_id_selected.Text = null;
                this.textBox_r_runs_script_id.Text = null;

                this.richTextBox_r_de.Text = null;
                this.richTextBox_r_en.Text = null;
                this.richTextBox_r_pl.Text = null;

                this.comboBox_r_condition_result.SelectedIndex = (short)Condition_result.NONE;

                //this.listBox_r_caused_by.Items.Clear();
            }






            // Enabling or disabling.
            this.checkBox_r_choose.Enabled = editing_ && exist;
            this.checkBox_r_conditional.Enabled = editing_ && exist;
            this.checkBox_r_runs_script.Enabled = editing_ && exist;
            this.checkBox_r_translation.Enabled = !editing_ && exist;
            this.checkBox_r_a_continue.Enabled = editing_ && exist && this.checkBox_r_choose.Checked;

            this.button_r_open.Enabled = exist;
            this.button_r_add_as_leads_to.Enabled = editing_ && exist;
            //this.button_r_caused_by_down.Enabled = editing_ && exist;
            //this.button_r_caused_by_remove.Enabled = editing_ && exist;
            //this.button_r_caused_by_up.Enabled = editing_ && exist;
            //this.button_r_delete.Enabled = editing_ && exist;
            //this.button_r_id_change.Enabled = editing_ && exist;
            //this.button_r_leads_to_create.Enabled = editing_ && exist;
            //this.button_r_leads_to_down.Enabled = editing_ && exist;
            //this.button_r_leads_to_up.Enabled = editing_ && exist;
            //this.button_r_leads_to_remove.Enabled = editing_ && exist;
            //this.button_r_pure_new.Enabled = editing_;

            this.textBox_r_id_person.Enabled = editing_ && exist;
            this.textBox_r_id_selected.Enabled = editing_ && exist;
            this.textBox_r_condition_id.Enabled = editing_ && exist && this.checkBox_r_conditional.Checked;

            this.richTextBox_r_de.Enabled = exist;
            this.richTextBox_r_en.Enabled = exist;
            this.richTextBox_r_pl.Enabled = exist;

            this.comboBox_r_condition_result.Enabled = editing_ && exist
                && this.checkBox_r_conditional.Checked && !this.checkBox_r_choose.Checked;

            this.textBox_r_runs_script_id.Enabled = editing_ && exist && this.checkBox_r_runs_script.Checked;






            //Console.WriteLine("right panel> " + this.current_selected_right_id_);
        }


        protected void update_left_panel(string id)
        {
            // Validating id.
            bool exist = this.dialogues_.validate_id(id);
            Dialogue dialogue_ref = null;

            // Saving value.
            this.current_selected_left_id_ = exist ? string.Copy(id) : null;
            this.select_back_caused_by_leads_to();


            // Filling up with data.
            if (exist)
            {
                dialogue_ref = this.dialogues_.dialogues[id];

                this.checkBox_l_choose.Checked = dialogue_ref.auto_choose;
                this.checkBox_l_conditional.Checked = dialogue_ref.is_conditional;
                this.checkBox_l_runs_script.Checked = dialogue_ref.runs_script;
                this.checkBox_l_translation.Checked = dialogue_ref.translation_approved;
                this.checkBox_l_a_continue.Checked = dialogue_ref.auto_continue && dialogue_ref.auto_choose;

                this.textBox_l_condition_id.Text = dialogue_ref.condition_id;
                this.textBox_l_id_person.Text = dialogue_ref.person_id;
                this.textBox_l_id_selected.Text = dialogue_ref.id;
                this.textBox_l_runs_script_id.Text = dialogue_ref.runs_script_id;

                this.richTextBox_l_de.Text = dialogue_ref.de;
                this.richTextBox_l_en.Text = dialogue_ref.en;
                this.richTextBox_l_pl.Text = dialogue_ref.pl;

                this.comboBox_l_condition_result.SelectedIndex = (short)dialogue_ref.condition_result;
            }
            else
            {
                this.checkBox_l_choose.Checked = false;
                this.checkBox_l_conditional.Checked = false;
                this.checkBox_l_runs_script.Checked = false;
                this.checkBox_l_translation.Checked = false;
                this.checkBox_l_a_continue.Checked = false;

                this.textBox_l_condition_id.Text = null;
                this.textBox_l_id_person.Text = null;
                this.textBox_l_id_selected.Text = null;
                this.textBox_l_runs_script_id.Text = null;

                this.richTextBox_l_de.Text = null;
                this.richTextBox_l_en.Text = null;
                this.richTextBox_l_pl.Text = null;

                this.comboBox_l_condition_result.SelectedIndex = (short)Condition_result.NONE;

                //this.listBox_l_caused_by.Items.Clear();
            }






            // Enabling or disabling.
            this.checkBox_l_choose.Enabled = editing_ && exist;
            this.checkBox_l_conditional.Enabled = editing_ && exist;
            this.checkBox_l_runs_script.Enabled = editing_ && exist;
            this.checkBox_l_translation.Enabled = !editing_ && exist;
            this.checkBox_l_a_continue.Enabled = editing_ && exist && this.checkBox_l_choose.Checked;

            this.button_l_open.Enabled = exist;
            this.button_l_add_as_leads_to.Enabled = editing_ && exist;
            //this.button_l_caused_by_down.Enabled = editing_ && exist;
            //this.button_l_caused_by_remove.Enabled = editing_ && exist;
            //this.button_l_caused_by_up.Enabled = editing_ && exist;
            //this.button_l_delete.Enabled = editing_ && exist;
            //this.button_l_id_change.Enabled = editing_ && exist;
            //this.button_l_leads_to_create.Enabled = editing_ && exist;
            //this.button_l_leads_to_down.Enabled = editing_ && exist;
            //this.button_l_leads_to_up.Enabled = editing_ && exist;
            //this.button_l_leads_to_remove.Enabled = editing_ && exist;
            //this.button_l_pure_new.Enabled = editing_;

            this.textBox_l_id_person.Enabled = editing_ && exist;
            this.textBox_l_id_selected.Enabled = editing_ && exist;
            this.textBox_l_condition_id.Enabled = editing_ && exist && this.checkBox_l_conditional.Checked;

            this.richTextBox_l_de.Enabled = exist;
            this.richTextBox_l_en.Enabled = exist;
            this.richTextBox_l_pl.Enabled = exist;

            this.comboBox_l_condition_result.Enabled = editing_ && exist
                && this.checkBox_l_conditional.Checked && !this.checkBox_l_choose.Checked;

            this.textBox_l_runs_script_id.Enabled = editing_ && exist && this.checkBox_l_runs_script.Checked;
        }


        protected void update_caused_by_leads_to_lists(string id)
        {
            // Validating id.
            bool exist = this.dialogues_.validate_id(id);
            Dialogue dialogue_ref = null;


            this.listBox_m_leads_to.Items.Clear();
            this.listBox_m_caused_by.Items.Clear();

            if (exist)
            {
                dialogue_ref = this.dialogues_.dialogues[id];

                foreach (var item in dialogue_ref.caused_by)
                {
                    this.listBox_m_caused_by.Items.Add(item);
                }


                foreach (var item in dialogue_ref.leads_to)
                {
                    this.listBox_m_leads_to.Items.Add(item);
                }

                this.select_back_caused_by_leads_to();
            }
        }


        /// <summary>
        /// Updates listBox_m_search and selects id in it. 
        /// </summary>
        /// <param name="id">ID to select. Null for deselect all.</param>
        protected void update_main_search_list(string id)
        {
            string curr_selected = null;
            bool exist = this.dialogues_.validate_id(id);

            if (exist)
            {
                curr_selected = id;
            }

            // Clean up.
            this.listBox_m_search.Items.Clear();

            // Inserting new data into lists.
            var search_condition = Formatter.get_formatted_or_null_id(this.textBox_m_search.Text);
            if (search_condition == null) search_condition = "";
            foreach (var dialogue in dialogues_.dialogues)
            {
                if ((dialogue.Key).Contains(search_condition))
                {
                    bool condition_match = true;

                    if (this.checkBox_m_search_only_begins.Checked
                        && dialogue.Value.caused_by.Count > 0)
                        condition_match = false;

                    if (this.checkBox_m_search_translate.Checked
                        && dialogue.Value.translation_approved)
                        condition_match = false;


                    if (condition_match)
                    {
                        this.listBox_m_search.Items.Add(dialogue.Key);
                    }
                }
            }

            if (curr_selected != null)
            {
                for (int i = 0; i < this.listBox_m_search.Items.Count; i++)
                {
                    if (this.listBox_m_search.Items[i].ToString().Equals(curr_selected))
                    {
                        this.listBox_m_search.SetSelected(i, true);
                        break;
                    }
                }
            }
            else
            {
                this.listBox_m_search.SelectedIndex = -1;
            }
        }


        protected void update_right_search_list(string id)
        {
            string curr_selected = null;
            bool exist = this.dialogues_.validate_id(id);

            if (exist)
            {
                curr_selected = id;
            }

            // Clean up.
            this.listBox_r_search.Items.Clear();

            // Inserting new data into lists.
            var search_condition = Formatter.get_formatted_or_null_id(this.textBox_r_search.Text);
            if (search_condition == null) search_condition = "";
            foreach (var dialogue in dialogues_.dialogues)
            {
                if ((dialogue.Key).Contains(search_condition))
                {
                    bool condition_match = true;

                    if (this.checkBox_r_search_only_begins.Checked
                        && dialogue.Value.caused_by.Count > 0)
                        condition_match = false;

                    if (condition_match)
                    {
                        this.listBox_r_search.Items.Add(dialogue.Key);
                    }
                }
            }

            if (curr_selected != null)
            {
                for (int i = 0; i < this.listBox_r_search.Items.Count; i++)
                {
                    if (this.listBox_r_search.Items[i].ToString().Equals(curr_selected))
                    {
                        this.listBox_r_search.SetSelected(i, true);
                        break;
                    }
                }
            }
            else
            {
                this.listBox_r_search.SelectedIndex = -1;
            }
        }


        protected void update_left_search_list(string id)
        {
            string curr_selected = null;
            bool exist = this.dialogues_.validate_id(id);

            if (exist)
            {
                curr_selected = id;
            }

            // Clean up.
            this.listBox_l_search.Items.Clear();

            // Inserting new data into lists.
            var search_condition = Formatter.get_formatted_or_null_id(this.textBox_l_search.Text);
            if (search_condition == null) search_condition = "";
            foreach (var dialogue in dialogues_.dialogues)
            {
                if ((dialogue.Key).Contains(search_condition))
                {
                    bool condition_match = true;

                    if (this.checkBox_l_search_only_begins.Checked
                        && dialogue.Value.caused_by.Count > 0)
                        condition_match = false;

                    if (condition_match)
                    {
                        this.listBox_l_search.Items.Add(dialogue.Key);
                    }
                }
            }

            if (curr_selected != null)
            {
                for (int i = 0; i < this.listBox_l_search.Items.Count; i++)
                {
                    if (this.listBox_l_search.Items[i].ToString().Equals(curr_selected))
                    {
                        this.listBox_l_search.SetSelected(i, true);
                        break;
                    }
                }
            }
            else
            {
                this.listBox_l_search.SelectedIndex = -1;
            }
        }


        protected void add_to_recently_opened(string id)
        {
            if (this.dialogues_.validate_id(id))
            {
                // Delete dialogue from recently opened dialogues list.
                this.listBox_m_recently.Items.Remove(id);

                // Add it at the very top position.
                this.listBox_m_recently.Items.Insert(0, id);

                // Remove all dialogues that indexes are above max_recently_opened_.
                for (int i = max_recently_opened_; i < this.listBox_m_recently.Items.Count; i++)
                {
                    this.listBox_m_recently.Items.RemoveAt(i);
                }

                this.listBox_m_recently.SelectedIndex = 0;
            }
            else
            {
                this.listBox_m_recently.SelectedIndex = -1;
            }
        }


        protected void select_back_caused_by_leads_to()
        {
            // Selects back item. If possible.
            if (this.current_selected_left_id_ != null)
            {
                for (int i = 0; i < this.listBox_m_caused_by.Items.Count; i++)
                {
                    if (this.listBox_m_caused_by.Items[i].ToString().Equals(this.current_selected_left_id_))
                    {
                        this.listBox_m_caused_by.SetSelected(i, true);
                        break;
                    }
                }
            }

            if (this.current_selected_right_id_ != null)
            {
                for (int i = 0; i < this.listBox_m_leads_to.Items.Count; i++)
                {
                    if (this.listBox_m_leads_to.Items[i].ToString().Equals(this.current_selected_right_id_))
                    {
                        this.listBox_m_leads_to.SetSelected(i, true);
                        break;
                    }
                }
            }
        }

        /// <summary>
        /// TODO: Finish this function.
        /// </summary>
        protected void revalidate_all_database()
        {
            // Firstly IDs.
            foreach (var element in this.dialogues_.dialogues)
            {

            }
        }







        /////////////////////////////////////////////////////////
        //  Events
        /////////////////////////////////////////////////////////

        private void translationModeToolStripMenuItem_Click(object sender, EventArgs e)
        {
            this.translationModeToolStripMenuItem.Checked = !this.translationModeToolStripMenuItem.Checked;
            this.editing_ = !this.translationModeToolStripMenuItem.Checked;

            this.update_main_panel(this.current_selected_main_id_);
            //TODO: Update left and right panels.
        }

        private void openToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Select nothing.
            this.label_m_id_selected.Select();

            // Setting up new open file dialog window.
            OpenFileDialog fd = new OpenFileDialog();
            fd.DefaultExt = "*.xml";

            // Opening, and awaiting for the resoult.
            DialogResult dr = fd.ShowDialog();

            if (dr == DialogResult.OK)
            {
                Stream stream = null;

                try
                {
                    if ((stream = fd.OpenFile()) != null)
                    {
                        // Firstly, try to load data from .json file.
                        StreamReader reader = new StreamReader(stream, Encoding.UTF8);
                        string value = reader.ReadToEnd();

                        // Deserialize data.
                        this.dialogues_ = (new JavaScriptSerializer()).Deserialize<Dialogues_Collection>(value);
                        // If it doesnt fail, save other information.


                        // Saving other data, and refreshing lists. 
                        this.file_path_ = fd.FileName;
                        this.file_path_name_ = fd.SafeFileName;
                        this.file_can_save_ = true;
                        this.changed_ = false;

                        // Refreshing lists values.
                        this.listBox_m_recently.Items.Clear();

                        this.update_main_panel(null);
                        this.update_main_search_list(null);
                        this.update_menu_state();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: Could not read file from disk. \nOriginal error: " + ex.Message, "Error");
                }
                finally
                {
                    stream.Close();
                }
            }
        }

        private void saveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Select nothing.
            this.label_m_id_selected.Select();

            StreamWriter stream = null;
            this.file_can_save_ = false;

            try
            {
                stream = new StreamWriter(
                    new FileStream(this.file_path_, FileMode.Create),
                    Encoding.UTF8
                    );
                string json_resoult = (new JavaScriptSerializer()).Serialize(this.dialogues_);

                stream.Write(json_resoult);

                this.file_can_save_ = true;

                this.changed_ = false;

                // Updating menu.
                this.update_menu_state();

                Console.WriteLine("Saved to: " + this.file_path_);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error: Could not save on disk. \nOriginal error: " + ex.Message, "Error");
            }
            finally
            {
                stream.Close();
            }
        }

        private void saveAsToolStripMenuItem_Click(object sender, EventArgs e)
        {
            // Select nothing.
            this.label_m_id_selected.Select();

            // Setting up new open file dialog window.
            SaveFileDialog fd = new SaveFileDialog();
            fd.DefaultExt = "*.xml";

            // Opening, and awaiting for the resoult.
            DialogResult dr = fd.ShowDialog();


            if (dr == DialogResult.OK)
            {
                Stream stream = null;
                StreamWriter streamWriter = null;

                string file_path;

                try
                {
                    if ((stream = fd.OpenFile()) != null)
                    {
                        // Getting the name of file.
                        file_path = string.Copy(fd.FileName);
                        stream.Close();

                        // Creating new file with this name.
                        streamWriter = new StreamWriter(
                            new FileStream(file_path, FileMode.Create),
                            Encoding.UTF8
                        );

                        // Saving json as a string.
                        string json_resoult = (new JavaScriptSerializer()).Serialize(this.dialogues_);

                        // Saving json to file.
                        streamWriter.Write(json_resoult);

                        // Saving values.
                        this.file_can_save_ = true;
                        this.file_path_ = file_path;
                        var splited = this.file_path_.Split('\\');
                        this.file_path_name_ = splited[splited.Length - 1];

                        this.changed_ = false;

                        // Updating menu.
                        this.update_menu_state();

                        Console.WriteLine("Saved to: " + this.file_path_);
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error: Could not save on disk. \nOriginal error: " + ex.Message, "Error");
                }
                finally
                {
                    streamWriter.Close();
                }
            }
        }



        private void textBox_m_search_KeyUp(object sender, KeyEventArgs e)
        {
            this.update_main_search_list(this.current_selected_main_id_);
        }

        private void listBox_m_search_Click(object sender, EventArgs e)
        {
            // Select nothing.
            this.label_m_id_selected.Select();

            string curr_selected = null;

            if (this.listBox_m_search.SelectedIndex != -1)
            {
                curr_selected = this.listBox_m_search.Items[this.listBox_m_search.SelectedIndex].ToString();
            }

            this.update_main_panel(curr_selected);
            this.add_to_recently_opened(curr_selected);
        }

        private void checkBox_m_search_only_begins_CheckedChanged(object sender, EventArgs e)
        {
            this.update_main_search_list(this.current_selected_main_id_);
        }

        private void checkBox_m_search_translate_CheckedChanged(object sender, EventArgs e)
        {
            this.update_main_search_list(this.current_selected_main_id_);
        }

        private void button_m_id_change_Click(object sender, EventArgs e)
        {
            // Select nothing.
            this.label_m_id_selected.Select();

            string text = Formatter.get_formatted_or_null_id(this.textBox_m_id_selected.Text);

            if (this.dialogues_.change_id(this.current_selected_main_id_, text))
            {
                // Delete dialogue from recently opened dialogues list.
                this.listBox_m_recently.Items.Remove(this.current_selected_main_id_);
                this.add_to_recently_opened(text);

                // Update values.
                this.update_main_panel(text);
                this.update_main_search_list(text);

                this.changed_ = true;
                this.update_menu_state();
            }
            else
            {
                MessageBox.Show("Id invaild or already in use", "Error");
            }

        }

        private void listBox_m_recently_Click(object sender, EventArgs e)
        {
            // Select nothing.
            this.label_m_id_selected.Select();

            int index = this.listBox_m_recently.SelectedIndex;

            if (index != -1)
            {
                string copy = this.listBox_m_recently.Items[index].ToString();

                this.update_main_panel(copy);
                this.update_main_search_list(copy);
            }
        }

        private void button_m_delete_Click(object sender, EventArgs e)
        {
            // Select nothing.
            this.label_m_id_selected.Select();

            // Make sure that it wasn't a missclick.
            DialogResult dialogResult = MessageBox.Show(
                "Are you sure you want to delete: " + this.current_selected_main_id_ + "?",
                "U sure dude?", MessageBoxButtons.YesNo);
            if (dialogResult == DialogResult.Yes)
            {
                this.dialogues_.remove(this.current_selected_main_id_);
                this.listBox_m_recently.Items.Remove(this.current_selected_main_id_);

                this.current_selected_main_id_ = null;

                // Update values.
                this.update_main_panel(null);
                this.update_main_search_list(null);

                this.changed_ = true;
                this.update_menu_state();
            }
        }

        private void button_m_pure_new_Click(object sender, EventArgs e)
        {
            // Select nothing.
            this.label_m_id_selected.Select();

            var dialogue = new Dialogue() { id = this.dialogues_.generate_unique_id() };

            this.dialogues_.add(dialogue);

            // Update values.
            this.update_main_panel(dialogue.id);
            this.update_main_search_list(dialogue.id);
            this.add_to_recently_opened(dialogue.id);

            this.changed_ = true;
            this.update_menu_state();
        }

        private void textBox_m_id_person_Leave(object sender, EventArgs e)
        {
            var dialogue = this.dialogues_.dialogues[this.current_selected_main_id_];
            //string text = (this.textBox_m_id_person.Text == null || this.textBox_m_id_person.Text.Equals("")) ?
            //    null : this.textBox_m_id_person.Text;

            string text = Formatter.get_formatted_or_null_id(this.textBox_m_id_person.Text);


            if ((text != null && !text.Equals(dialogue.person_id))
                || (dialogue.person_id != null && text == null))
            {
                this.changed_ = true;
                dialogue.person_id = text;

                this.update_menu_state();
            }

            this.textBox_m_id_person.Text = text;
        }

        private void richTextBox_m_pl_Leave(object sender, EventArgs e)
        {
            var dialogue = this.dialogues_.dialogues[this.current_selected_main_id_];
            //string text = (this.richTextBox_m_pl.Text == null || this.richTextBox_m_pl.Text.Equals("")) ?
            //    null : this.richTextBox_m_pl.Text;
            string text = Formatter.get_formatted_or_null_text(this.richTextBox_m_pl.Text);

            if ((text != null && !text.Equals(dialogue.pl))
                || (dialogue.pl != null && text == null))
            {
                this.changed_ = true;
                dialogue.pl = text;

                this.update_menu_state();
            }

            this.richTextBox_m_pl.Text = text;
        }

        private void richTextBox_m_en_Leave(object sender, EventArgs e)
        {
            var dialogue = this.dialogues_.dialogues[this.current_selected_main_id_];
            //string text = (this.richTextBox_m_en.Text == null || this.richTextBox_m_en.Text.Equals("")) ?
            //    null : this.richTextBox_m_en.Text;
            string text = Formatter.get_formatted_or_null_text(this.richTextBox_m_en.Text);

            if ((text != null && !text.Equals(dialogue.en))
                || (dialogue.en != null && text == null))
            {
                this.changed_ = true;
                dialogue.en = text;

                this.update_menu_state();
            }

            this.richTextBox_m_en.Text = text;
        }

        private void richTextBox_m_de_Leave(object sender, EventArgs e)
        {
            var dialogue = this.dialogues_.dialogues[this.current_selected_main_id_];
            //string text = (this.richTextBox_m_de.Text == null || this.richTextBox_m_de.Text.Equals("")) ?
            //    null : this.richTextBox_m_de.Text;
            string text = Formatter.get_formatted_or_null_text(this.richTextBox_m_de.Text);

            if ((text != null && !text.Equals(dialogue.de))
                || (dialogue.de != null && text == null))
            {
                this.changed_ = true;
                dialogue.de = text;

                this.update_menu_state();
            }

            this.richTextBox_m_de.Text = text;
        }

        private void checkBox_m_translation_Click(object sender, EventArgs e)
        {
            // Select nothing.
            this.label_m_id_selected.Select();

            //this.checkBox_m_translation.Checked = !this.checkBox_m_translation.Checked;
            this.changed_ = true;
            this.dialogues_.dialogues[this.current_selected_main_id_].translation_approved = this.checkBox_m_translation.Checked;

            this.update_main_panel(this.current_selected_main_id_);
            this.update_main_search_list(this.current_selected_main_id_);
            //TODO: Update left and right panels.
            this.update_menu_state();
        }

        private void checkBox_m_choose_Click(object sender, EventArgs e)
        {
            // Select nothing.
            this.label_m_id_selected.Select();

            //this.checkBox_m_choose.Checked = !this.checkBox_m_choose.Checked;
            this.changed_ = true;
            this.dialogues_.dialogues[this.current_selected_main_id_].auto_choose = this.checkBox_m_choose.Checked;

            this.update_main_panel(this.current_selected_main_id_);
            //TODO: Update left and right panels.
            this.update_menu_state();
        }

        private void checkBox_m_conditional_Click(object sender, EventArgs e)
        {
            // Select nothing.
            this.label_m_id_selected.Select();

            //this.checkBox_m_conditional.Checked = !this.checkBox_m_conditional.Checked;
            this.changed_ = true;
            this.dialogues_.dialogues[this.current_selected_main_id_].is_conditional = this.checkBox_m_conditional.Checked;

            this.update_main_panel(this.current_selected_main_id_);
            //TODO: Update left and right panels.
            this.update_menu_state();
        }

        private void checkBox_m_runs_script_Click(object sender, EventArgs e)
        {
            // Select nothing.
            this.label_m_id_selected.Select();

            //this.checkBox_m_runs_script.Checked = !this.checkBox_m_runs_script.Checked;
            this.changed_ = true;
            this.dialogues_.dialogues[this.current_selected_main_id_].runs_script = this.checkBox_m_runs_script.Checked;

            this.update_main_panel(this.current_selected_main_id_);
            //TODO: Update left and right panels.
            this.update_menu_state();
        }

        private void textBox_m_condition_id_Leave(object sender, EventArgs e)
        {
            var dialogue = this.dialogues_.dialogues[this.current_selected_main_id_];
            //string text = (this.textBox_m_condition_id.Text == null || this.textBox_m_condition_id.Text.Equals("")) ?
            //    null : this.textBox_m_condition_id.Text;

            string text = Formatter.get_formatted_or_null_id(this.textBox_m_condition_id.Text);

            if ((text != null && !text.Equals(dialogue.condition_id))
                || (dialogue.condition_id != null && text == null))
            {
                this.changed_ = true;
                dialogue.condition_id = text;

                this.update_menu_state();
            }

            this.textBox_m_condition_id.Text = text;
        }

        private void textBox_m_runs_script_id_Leave(object sender, EventArgs e)
        {
            var dialogue = this.dialogues_.dialogues[this.current_selected_main_id_];
            //string text = (this.textBox_m_runs_script_id.Text == null || this.textBox_m_runs_script_id.Text.Equals("")) ?
            //    null : this.textBox_m_runs_script_id.Text;
            string text = Formatter.get_formatted_or_null_id(this.textBox_m_runs_script_id.Text);

            if ((text != null && !text.Equals(dialogue.runs_script_id))
                || (dialogue.runs_script_id != null && text == null))
            {
                this.changed_ = true;
                dialogue.runs_script_id = text;

                this.update_menu_state();
            }

            this.textBox_m_runs_script_id.Text = text;
        }

        private void comboBox_m_condition_result_Leave(object sender, EventArgs e)
        {
            var dialogue = this.dialogues_.dialogues[this.current_selected_main_id_];
            int index = this.comboBox_m_condition_result.SelectedIndex;

            if (index != -1 && index != (short)dialogue.condition_result)
            {
                dialogue.condition_result = (Condition_result)index;
                this.changed_ = true;

                this.update_menu_state();
            }
        }

        private void button_m_leads_to_up_Click(object sender, EventArgs e)
        {
            // Select nothing.
            this.label_m_id_selected.Select();


            if (this.current_selected_main_id_ != null)
            {
                int index = this.listBox_m_leads_to.SelectedIndex;
                bool succes = this.dialogues_.dialogues[this.current_selected_main_id_].swap_leads_to(index, index - 1);

                if (succes)
                {
                    //this.update_main_panel(this.current_selected_main_id_);
                    this.update_caused_by_leads_to_lists(this.current_selected_main_id_);
                    //Console.WriteLine("listBox up main> " + this.current_selected_main_id_);
                    //Console.WriteLine("listBox up right> " + this.current_selected_right_id_);

                    this.changed_ = true;
                    this.update_menu_state();
                }
            }
        }

        private void button_m_leads_to_down_Click(object sender, EventArgs e)
        {
            // Select nothing.
            this.label_m_id_selected.Select();

            if (this.current_selected_main_id_ != null)
            {
                int index = this.listBox_m_leads_to.SelectedIndex;
                bool succes = this.dialogues_.dialogues[this.current_selected_main_id_].swap_leads_to(index, index + 1);

                if (succes)
                {
                    //this.update_main_panel(this.current_selected_main_id_);
                    this.update_caused_by_leads_to_lists(this.current_selected_main_id_);
                    //Console.WriteLine("listBox down main> " + this.current_selected_main_id_);
                    //Console.WriteLine("listBox down right> " + this.current_selected_right_id_);

                    this.changed_ = true;
                    this.update_menu_state();
                }
            }
        }

        private void listBox_m_leads_to_Click(object sender, EventArgs e)
        {
            // Select nothing.
            this.label_m_id_selected.Select();

            int index = this.listBox_m_leads_to.SelectedIndex;

            if (index != -1)
            {
                string copy = this.listBox_m_leads_to.Items[index].ToString();

                if (this.dialogues_.validate_id(copy))
                {
                    this.update_right_panel(copy);
                    this.update_right_search_list(this.current_selected_right_id_);

                }
                else
                {
                    // Not in database.
                    DialogResult dialogResult = MessageBox.Show(
                        "Dialogue possibly broken, ID: " + this.listBox_m_leads_to.Items[index].ToString() + " does not exit.\n\n"
                        + "Would you like to take an attempt to fix indexing?\n(remove anything that not exist in current database)",
                        "Error", MessageBoxButtons.YesNo);

                    if (dialogResult == DialogResult.Yes)
                    {
                        this.revalidate_all_database();

                        // Update values.
                        this.update_main_panel(null);
                        this.update_main_search_list(null);

                        this.changed_ = true;
                        this.update_menu_state();
                    }
                }
            }
            else
            {
                this.current_selected_right_id_ = null;
            }

            //Console.WriteLine("listBox select> " + this.current_selected_right_id_);
        }

        private void button_m_leads_to_remove_Click(object sender, EventArgs e)
        {
            // Select nothing.
            this.label_m_id_selected.Select();

            int index = this.listBox_m_leads_to.SelectedIndex;

            if (index != -1)
            {
                string id = this.listBox_m_leads_to.Items[index].ToString();

                if (this.dialogues_.validate_id(id))
                {
                    Dialogue dialogue_ref = this.dialogues_.dialogues[id];

                    dialogue_ref.caused_by.Remove(this.current_selected_main_id_);

                    this.dialogues_.dialogues[this.current_selected_main_id_].leads_to.Remove(id);

                    this.changed_ = true;
                    this.update_caused_by_leads_to_lists(this.current_selected_main_id_);
                    this.update_menu_state();
                }
            }
        }

        private void button_m_caused_by_remove_Click(object sender, EventArgs e)
        {
            // Select nothing.
            this.label_m_id_selected.Select();

            int index = this.listBox_m_caused_by.SelectedIndex;

            if (index != -1)
            {
                string id = this.listBox_m_caused_by.Items[index].ToString();

                if (this.dialogues_.validate_id(id))
                {
                    Dialogue dialogue_ref = this.dialogues_.dialogues[id];

                    dialogue_ref.leads_to.Remove(this.current_selected_main_id_);

                    this.dialogues_.dialogues[this.current_selected_main_id_].caused_by.Remove(id);

                    this.changed_ = true;
                    this.update_caused_by_leads_to_lists(this.current_selected_main_id_);
                    this.update_menu_state();
                }
            }
        }

        private void button_m_leads_to_create_Click(object sender, EventArgs e)
        {
            // Select nothing.
            this.label_m_id_selected.Select();

            var dialogue = new Dialogue() { id = this.dialogues_.generate_unique_id() };

            this.dialogues_.add(dialogue);
            this.dialogues_.create_link(this.current_selected_main_id_, dialogue.id);

            // Update values.
            this.update_main_panel(dialogue.id);
            this.update_main_search_list(dialogue.id);
            this.add_to_recently_opened(dialogue.id);

            this.changed_ = true;
            this.update_menu_state();
        }

        private void checkBox_m_a_continue_Click(object sender, EventArgs e)
        {
            // Select nothing.
            this.label_m_id_selected.Select();

            //this.checkBox_m_conditional.Checked = !this.checkBox_m_conditional.Checked;
            this.changed_ = true;
            this.dialogues_.dialogues[this.current_selected_main_id_].auto_continue = this.checkBox_m_a_continue.Checked;

            this.update_main_panel(this.current_selected_main_id_);
            //TODO: Update left and right panels.
            this.update_menu_state();
        }

        private void listBox_m_caused_by_Click(object sender, EventArgs e)
        {
            // Select nothing.
            this.label_m_id_selected.Select();

            int index = this.listBox_m_caused_by.SelectedIndex;

            if (index != -1)
            {
                string copy = this.listBox_m_caused_by.Items[index].ToString();

                if (this.dialogues_.validate_id(copy))
                {
                    this.update_left_panel(copy);
                    this.update_left_search_list(this.current_selected_left_id_);
                }
                else
                {
                    // Not in database.
                    DialogResult dialogResult = MessageBox.Show(
                        "Dialogue possibly broken, ID: " + this.listBox_m_caused_by.Items[index].ToString() + " does not exit.\n\n"
                        + "Would you like to take an attempt to fix indexing?\n(remove anything that not exist in current database)",
                        "Error", MessageBoxButtons.YesNo);

                    if (dialogResult == DialogResult.Yes)
                    {
                        this.revalidate_all_database();

                        // Update values.
                        this.update_main_panel(null);
                        this.update_main_search_list(null);

                        this.changed_ = true;
                        this.update_menu_state();
                    }
                }
            }
            else
            {
                this.current_selected_left_id_ = null;
            }
        }

        private void listBox_m_leads_to_DoubleClick(object sender, EventArgs e)
        {
            // Select nothing.
            this.label_m_id_selected.Select();

            int index = this.listBox_m_leads_to.SelectedIndex;

            if (index != -1)
            {
                string copy = this.listBox_m_leads_to.Items[index].ToString();

                if (this.dialogues_.validate_id(copy))
                {
                    // Update values.
                    this.update_main_panel(copy);
                    this.update_main_search_list(copy);
                    this.add_to_recently_opened(copy);
                }
                else
                {
                    // Not in database.
                    DialogResult dialogResult = MessageBox.Show(
                        "Dialogue possibly broken, ID: " + this.listBox_m_leads_to.Items[index].ToString() + " does not exit.\n\n"
                        + "Would you like to take an attempt to fix indexing?\n(remove anything that not exist in current database)",
                        "Error", MessageBoxButtons.YesNo);

                    if (dialogResult == DialogResult.Yes)
                    {
                        this.revalidate_all_database();

                        // Update values.
                        this.update_main_panel(null);
                        this.update_main_search_list(null);

                        this.changed_ = true;
                        this.update_menu_state();
                    }
                }
            }
        }

        private void listBox_m_caused_by_DoubleClick(object sender, EventArgs e)
        {
            // Select nothing.
            this.label_m_id_selected.Select();

            int index = this.listBox_m_caused_by.SelectedIndex;

            if (index != -1)
            {
                string copy = this.listBox_m_caused_by.Items[index].ToString();

                if (this.dialogues_.validate_id(copy))
                {
                    // Update values.
                    this.update_main_panel(copy);
                    this.update_main_search_list(copy);
                    this.add_to_recently_opened(copy);
                }
                else
                {
                    // Not in database.
                    DialogResult dialogResult = MessageBox.Show(
                        "Dialogue possibly broken, ID: " + this.listBox_m_caused_by.Items[index].ToString() + " does not exit.\n\n"
                        + "Would you like to take an attempt to fix indexing?\n(remove anything that not exist in current database)",
                        "Error", MessageBoxButtons.YesNo);

                    if (dialogResult == DialogResult.Yes)
                    {
                        this.revalidate_all_database();

                        // Update values.
                        this.update_main_panel(null);
                        this.update_main_search_list(null);

                        this.changed_ = true;
                        this.update_menu_state();
                    }
                }
            }
        }



        private void button_r_open_Click(object sender, EventArgs e)
        {
            // Select nothing.
            this.label_m_id_selected.Select();

            if (this.dialogues_.validate_id(this.current_selected_right_id_))
            {
                this.update_main_search_list(this.current_selected_right_id_);
                this.add_to_recently_opened(this.current_selected_right_id_);
                this.update_main_panel(this.current_selected_right_id_);
                //this.add_to_recently_opened(this.current_selected_right_id_);
            }
        }

        private void listBox_r_search_Click(object sender, EventArgs e)
        {
            // Select nothing.
            this.label_m_id_selected.Select();

            string curr_selected = null;

            if (this.listBox_r_search.SelectedIndex != -1)
            {
                curr_selected = this.listBox_r_search.Items[this.listBox_r_search.SelectedIndex].ToString();
            }

            this.update_right_panel(curr_selected);
        }

        private void textBox_r_search_KeyUp(object sender, KeyEventArgs e)
        {
            this.update_right_search_list(this.current_selected_right_id_);
        }

        private void checkBox_r_search_only_begins_Click(object sender, EventArgs e)
        {
            this.update_right_search_list(this.current_selected_right_id_);
        }

        private void button_r_add_as_leads_to_Click(object sender, EventArgs e)
        {
            // Select nothing.
            this.label_m_id_selected.Select();

            if (this.dialogues_.create_link(this.current_selected_main_id_, this.current_selected_right_id_))
            {
                this.update_caused_by_leads_to_lists(this.current_selected_main_id_);
            }
            else
            {
                MessageBox.Show("Cannot link the dialogue with the same dialogue\n"
                    + "(Would lead to infinite loop.)", "Error");
            }
        }

        private void textBox_r_id_person_Leave(object sender, EventArgs e)
        {
            var dialogue = this.dialogues_.dialogues[this.current_selected_right_id_];

            string text = Formatter.get_formatted_or_null_id(this.textBox_r_id_person.Text);


            if ((text != null && !text.Equals(dialogue.person_id))
                || (dialogue.person_id != null && text == null))
            {
                this.changed_ = true;
                dialogue.person_id = text;

                this.update_menu_state();
            }

            this.textBox_r_id_person.Text = text;
        }

        private void checkBox_r_translation_Click(object sender, EventArgs e)
        {
            // Select nothing.
            this.label_m_id_selected.Select();

            this.changed_ = true;
            this.dialogues_.dialogues[this.current_selected_right_id_].translation_approved = this.checkBox_r_translation.Checked;

            this.update_right_panel(this.current_selected_right_id_);
            this.update_main_search_list(this.current_selected_main_id_);
            this.update_right_search_list(this.current_selected_right_id_);
            this.update_menu_state();
        }

        private void richTextBox_r_pl_Leave(object sender, EventArgs e)
        {
            var dialogue = this.dialogues_.dialogues[this.current_selected_right_id_];
            string text = Formatter.get_formatted_or_null_text(this.richTextBox_r_pl.Text);

            if ((text != null && !text.Equals(dialogue.pl))
                || (dialogue.pl != null && text == null))
            {
                this.changed_ = true;
                dialogue.pl = text;

                this.update_menu_state();
            }

            this.richTextBox_r_pl.Text = text;
        }

        private void richTextBox_r_en_Leave(object sender, EventArgs e)
        {
            var dialogue = this.dialogues_.dialogues[this.current_selected_right_id_];
            string text = Formatter.get_formatted_or_null_text(this.richTextBox_r_en.Text);

            if ((text != null && !text.Equals(dialogue.en))
                || (dialogue.en != null && text == null))
            {
                this.changed_ = true;
                dialogue.en = text;

                this.update_menu_state();
            }

            this.richTextBox_r_en.Text = text;

        }

        private void richTextBox_r_de_Leave(object sender, EventArgs e)
        {
            var dialogue = this.dialogues_.dialogues[this.current_selected_right_id_];
            string text = Formatter.get_formatted_or_null_text(this.richTextBox_r_de.Text);

            if ((text != null && !text.Equals(dialogue.de))
                || (dialogue.de != null && text == null))
            {
                this.changed_ = true;
                dialogue.de = text;

                this.update_menu_state();
            }

            this.richTextBox_r_de.Text = text;
        }

        private void checkBox_r_choose_Click(object sender, EventArgs e)
        {
            // Select nothing.
            this.label_m_id_selected.Select();

            //this.checkBox_m_choose.Checked = !this.checkBox_m_choose.Checked;
            this.changed_ = true;
            this.dialogues_.dialogues[this.current_selected_right_id_].auto_choose = this.checkBox_r_choose.Checked;

            this.update_right_panel(this.current_selected_right_id_);
            this.update_menu_state();
        }

        private void checkBox_r_conditional_Click(object sender, EventArgs e)
        {
            // Select nothing.
            this.label_m_id_selected.Select();

            this.changed_ = true;
            this.dialogues_.dialogues[this.current_selected_right_id_].is_conditional = this.checkBox_r_conditional.Checked;

            this.update_right_panel(this.current_selected_right_id_);
            this.update_menu_state();
        }

        private void textBox_r_condition_id_Leave(object sender, EventArgs e)
        {
            var dialogue = this.dialogues_.dialogues[this.current_selected_right_id_];

            string text = Formatter.get_formatted_or_null_id(this.textBox_r_condition_id.Text);

            if ((text != null && !text.Equals(dialogue.condition_id))
                || (dialogue.condition_id != null && text == null))
            {
                this.changed_ = true;
                dialogue.condition_id = text;

                this.update_menu_state();
            }

            this.textBox_r_condition_id.Text = text;
        }

        private void comboBox_r_condition_result_Leave(object sender, EventArgs e)
        {
            var dialogue = this.dialogues_.dialogues[this.current_selected_right_id_];
            int index = this.comboBox_r_condition_result.SelectedIndex;

            if (index != -1 && index != (short)dialogue.condition_result)
            {
                dialogue.condition_result = (Condition_result)index;
                this.changed_ = true;

                this.update_menu_state();
            }
        }

        private void checkBox_r_runs_script_Click(object sender, EventArgs e)
        {
            // Select nothing.
            this.label_m_id_selected.Select();

            this.changed_ = true;
            this.dialogues_.dialogues[this.current_selected_right_id_].runs_script = this.checkBox_r_runs_script.Checked;

            this.update_right_panel(this.current_selected_right_id_);
            this.update_menu_state();
        }

        private void textBox_r_runs_script_id_Leave(object sender, EventArgs e)
        {
            // Select nothing.
            this.label_m_id_selected.Select();

            var dialogue = this.dialogues_.dialogues[this.current_selected_right_id_];

            string text = Formatter.get_formatted_or_null_id(this.textBox_r_runs_script_id.Text);

            if ((text != null && !text.Equals(dialogue.runs_script_id))
                || (dialogue.runs_script_id != null && text == null))
            {
                this.changed_ = true;
                dialogue.runs_script_id = text;

                this.update_menu_state();
            }

            this.textBox_r_runs_script_id.Text = text;
        }

        private void checkBox_r_a_continue_Click(object sender, EventArgs e)
        {
            // Select nothing.
            this.label_m_id_selected.Select();

            this.changed_ = true;
            this.dialogues_.dialogues[this.current_selected_right_id_].auto_continue = this.checkBox_r_a_continue.Checked;

            this.update_right_panel(this.current_selected_right_id_);
            this.update_menu_state();
        }


        private void button_l_open_Click(object sender, EventArgs e)
        {
            // Select nothing.
            this.label_m_id_selected.Select();

            if (this.dialogues_.validate_id(this.current_selected_left_id_))
            {
                this.update_main_search_list(this.current_selected_left_id_);
                this.add_to_recently_opened(this.current_selected_left_id_);
                this.update_main_panel(this.current_selected_left_id_);
                //this.add_to_recently_opened(this.current_selected_left_id_);
            }
        }

        private void listBox_l_search_Click(object sender, EventArgs e)
        {
            // Select nothing.
            this.label_m_id_selected.Select();

            string curr_selected = null;

            if (this.listBox_l_search.SelectedIndex != -1)
            {
                curr_selected = this.listBox_l_search.Items[this.listBox_l_search.SelectedIndex].ToString();
            }

            this.update_left_panel(curr_selected);
        }

        private void textBox_l_search_KeyUp(object sender, KeyEventArgs e)
        {
            this.update_left_search_list(this.current_selected_left_id_);
        }

        private void checkBox_l_search_only_begins_Click(object sender, EventArgs e)
        {
            this.update_left_search_list(this.current_selected_left_id_);
        }

        private void button_l_add_as_caused_by_Click(object sender, EventArgs e)
        {
            // Select nothing.
            this.label_m_id_selected.Select();

            if (this.dialogues_.create_link(this.current_selected_left_id_, this.current_selected_main_id_))
            {
                this.update_caused_by_leads_to_lists(this.current_selected_main_id_);
            }
            else
            {
                MessageBox.Show("Cannot link the dialogue with the same dialogue\n"
                    + "(Would lead to infinite loop.)", "Error");
            }
        }

        private void textBox_l_id_person_Leave(object sender, EventArgs e)
        {
            var dialogue = this.dialogues_.dialogues[this.current_selected_left_id_];

            string text = Formatter.get_formatted_or_null_id(this.textBox_l_id_person.Text);


            if ((text != null && !text.Equals(dialogue.person_id))
                || (dialogue.person_id != null && text == null))
            {
                this.changed_ = true;
                dialogue.person_id = text;

                this.update_menu_state();
            }

            this.textBox_l_id_person.Text = text;
        }

        private void checkBox_l_translation_Click(object sender, EventArgs e)
        {
            // Select nothing.
            this.label_m_id_selected.Select();

            this.changed_ = true;
            this.dialogues_.dialogues[this.current_selected_left_id_].translation_approved = this.checkBox_l_translation.Checked;

            this.update_left_panel(this.current_selected_left_id_);
            this.update_main_search_list(this.current_selected_main_id_);
            this.update_left_search_list(this.current_selected_left_id_);
            this.update_menu_state();
        }

        private void richTextBox_l_pl_Leave(object sender, EventArgs e)
        {
            var dialogue = this.dialogues_.dialogues[this.current_selected_left_id_];
            string text = Formatter.get_formatted_or_null_text(this.richTextBox_l_pl.Text);

            if ((text != null && !text.Equals(dialogue.pl))
                || (dialogue.pl != null && text == null))
            {
                this.changed_ = true;
                dialogue.pl = text;

                this.update_menu_state();
            }

            this.richTextBox_l_pl.Text = text;
        }

        private void richTextBox_l_en_Leave(object sender, EventArgs e)
        {
            var dialogue = this.dialogues_.dialogues[this.current_selected_left_id_];
            string text = Formatter.get_formatted_or_null_text(this.richTextBox_l_en.Text);

            if ((text != null && !text.Equals(dialogue.en))
                || (dialogue.en != null && text == null))
            {
                this.changed_ = true;
                dialogue.en = text;

                this.update_menu_state();
            }

            this.richTextBox_l_en.Text = text;

        }

        private void richTextBox_l_de_Leave(object sender, EventArgs e)
        {
            var dialogue = this.dialogues_.dialogues[this.current_selected_left_id_];
            string text = Formatter.get_formatted_or_null_text(this.richTextBox_l_de.Text);

            if ((text != null && !text.Equals(dialogue.de))
                || (dialogue.de != null && text == null))
            {
                this.changed_ = true;
                dialogue.de = text;

                this.update_menu_state();
            }

            this.richTextBox_l_de.Text = text;
        }

        private void checkBox_l_choose_Click(object sender, EventArgs e)
        {
            // Select nothing.
            this.label_m_id_selected.Select();

            //this.checkBox_m_choose.Checked = !this.checkBox_m_choose.Checked;
            this.changed_ = true;
            this.dialogues_.dialogues[this.current_selected_left_id_].auto_choose = this.checkBox_l_choose.Checked;

            this.update_left_panel(this.current_selected_left_id_);
            this.update_menu_state();
        }

        private void checkBox_l_conditional_Click(object sender, EventArgs e)
        {
            // Select nothing.
            this.label_m_id_selected.Select();

            this.changed_ = true;
            this.dialogues_.dialogues[this.current_selected_left_id_].is_conditional = this.checkBox_l_conditional.Checked;

            this.update_left_panel(this.current_selected_left_id_);
            this.update_menu_state();
        }

        private void textBox_l_condition_id_Leave(object sender, EventArgs e)
        {
            var dialogue = this.dialogues_.dialogues[this.current_selected_left_id_];

            string text = Formatter.get_formatted_or_null_id(this.textBox_l_condition_id.Text);

            if ((text != null && !text.Equals(dialogue.condition_id))
                || (dialogue.condition_id != null && text == null))
            {
                this.changed_ = true;
                dialogue.condition_id = text;

                this.update_menu_state();
            }

            this.textBox_l_condition_id.Text = text;
        }

        private void comboBox_l_condition_result_Leave(object sender, EventArgs e)
        {
            var dialogue = this.dialogues_.dialogues[this.current_selected_left_id_];
            int index = this.comboBox_l_condition_result.SelectedIndex;

            if (index != -1 && index != (short)dialogue.condition_result)
            {
                dialogue.condition_result = (Condition_result)index;
                this.changed_ = true;

                this.update_menu_state();
            }
        }

        private void checkBox_l_runs_script_Click(object sender, EventArgs e)
        {
            // Select nothing.
            this.label_m_id_selected.Select();

            this.changed_ = true;
            this.dialogues_.dialogues[this.current_selected_left_id_].runs_script = this.checkBox_l_runs_script.Checked;

            this.update_left_panel(this.current_selected_left_id_);
            this.update_menu_state();
        }

        private void textBox_l_runs_script_id_Leave(object sender, EventArgs e)
        {
            // Select nothing.
            this.label_m_id_selected.Select();

            var dialogue = this.dialogues_.dialogues[this.current_selected_left_id_];

            string text = Formatter.get_formatted_or_null_id(this.textBox_l_runs_script_id.Text);

            if ((text != null && !text.Equals(dialogue.runs_script_id))
                || (dialogue.runs_script_id != null && text == null))
            {
                this.changed_ = true;
                dialogue.runs_script_id = text;

                this.update_menu_state();
            }

            this.textBox_l_runs_script_id.Text = text;
        }

        private void checkBox_l_a_continue_Click(object sender, EventArgs e)
        {
            // Select nothing.
            this.label_m_id_selected.Select();

            this.changed_ = true;
            this.dialogues_.dialogues[this.current_selected_left_id_].auto_continue = this.checkBox_l_a_continue.Checked;

            this.update_left_panel(this.current_selected_left_id_);
            this.update_menu_state();
        }







        //private void button1_Click(object sender, EventArgs e)
        //{
        //    List<int> lista = new List<int>();
        //    lista.Add(1);
        //    lista.Add(25);
        //    lista.Add(12452);
        //    lista.Add(3);
        //    lista.Add(74);

        //    /*
        //    foreach (var item in lista)
        //    {
        //        if (item == 25)
        //    }
        //    */

        //    lista.Where(item => item == 25); //Any // item => item.pole == 15







        //    // Jsonki:



        //    List<Person> person = new List<Person>();
        //    person.Add(new Person());
        //    person.Add(new Person());
        //    person.Add(new Person());
        //    person[1].age = 32;
        //    person[2].age = 18;
        //    person[1].Name = "Kot";

        //    //person.ForEach(item => item.age = 15);
        //    //if (person.Any()) person.RemoveAt(0);
        //    //if (person.Any(item => item.age == 15)) person.RemoveAt(0);

        //    var q = person.FirstOrDefault(item => item.age == 32);


        //    //Console.WriteLine("q: " + q);
        //    Console.WriteLine("person[1]: " + person[1].age);
        //    person[1].age = 99;
        //    Console.WriteLine("q: " + q.age);




        //    //JavaScriptSerializer json = new JavaScriptSerializer();
        //    string json_resoult = (new JavaScriptSerializer()).Serialize(person);

        //    Console.WriteLine(json_resoult);


        //    List<Person_reduced> json_deserialized = (new JavaScriptSerializer()).Deserialize<List<Person_reduced>>(json_resoult);

        //    foreach (var item in json_deserialized)
        //    {
        //        Console.WriteLine(item.Name);
        //    }
        //}





        //[Serializable]
        //public class Person
        //{
        //    public int age = 20;
        //    protected string name = "None";

        //    public string Name
        //    {
        //        get
        //        {
        //            return this.name;
        //        }

        //        set
        //        {
        //            if (value != null)
        //                this.name = value.ToUpper();
        //            else
        //                this.name = null;
        //        }
        //    }
        //}



        //[Serializable]
        //public class Person_reduced
        //{
        //    protected string name = "None";

        //    public string Name
        //    {
        //        get
        //        {
        //            return this.name;
        //        }

        //        set
        //        {
        //            if (value != null)
        //                this.name = value.ToUpper();
        //            else
        //                this.name = null;
        //        }
        //    }
        //}


    }
}
