using System;
using System.Net.Http;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;
using System.Net;
using System.Text.RegularExpressions;
using System.IO;
using System.Collections.Generic;
using System.Windows.Data;
using System.ComponentModel;
using System.Threading;

namespace MetaboliteViewer {

    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window {

        private System.Media.SoundPlayer simpleSound = new System.Media.SoundPlayer(@"Data\rur4t.wav");
        public List<string> TestItems { get; set; } = new List<string>();
        public List<string> Pathways { get; set; } = new List<string>();
        private string _statusMessage;
        public string StatusMessage {
            get {
                return _statusMessage;
            }
            set {
                _statusMessage = value;
                NotifyPropertyChanged("LastName");
            }
        }
        public string TestText { get; set; }
        bool isFinished = false;
        bool FunMode = false;

        public MainWindow() {
            InitializeComponent();
            TestItems.Add("H2O");
            StatusMessage = "TESTING";
            LabelTest.DataContext = _statusMessage;
            TestText = "TESTIHG";

            loadPathways();
            Thread myThread = new Thread(new ThreadStart(loadList));
            myThread.Start();
            while (!isFinished) {
                Thread.Sleep(1000);
            }
            DataContext = this;
        }

        /// <summary>
        /// Loads the list of compounds
        /// </summary>
        private async void loadList() {
            string result;
            string baseUrl = "http://rest.kegg.jp/list/compound";

            using (HttpClient client = new HttpClient()) {
                result = await client.GetStringAsync(baseUrl);
            }
            string[] splitshit = result.Split('\n');
            foreach (string item in splitshit) {
                string[] splitByTab = item.Split('\t');
                if(splitByTab.Length > 1) {
                    string[] splitBySemi = splitByTab[1].Split(';');
                    foreach (string compound in splitBySemi) {
                        TestItems.Add(compound.Trim());
                    }
                } else {

                }

            }
            isFinished = true;
        }

        /// <summary>
        /// Sets the image to the searched compound or pathway
        /// </summary>
        /// <param name="st">SearchTerm determining if we are looking for a compound or pathway</param>
        /// <returns></returns>
        private async Task GetFromKegg(SearchTerm st)
        {
            Cursor = Cursors.AppStarting;

            string result;
            string userInput;
            string baseURL = "http://rest.kegg.jp/list/";
            string databaseID = string.Empty;
            int subStringStart;
            int substringLength;

            if (st == SearchTerm.compound)
            {
                userInput = CompoundNameField.Text;
                baseURL += @"compound";
                substringLength = 6;
                subStringStart = 4;
            }
            else
            {
                userInput = pathwayField.Text;
                baseURL += @"pathway";
                subStringStart = 5;
                substringLength = 8;
                
            }

            using(HttpClient client = new HttpClient())
            {
                result = await client.GetStringAsync(baseURL);
            }

            string[] databaseRowArray = result.Split('\n');
            string pattern = @"\s" + userInput + @";";
            foreach (string item in databaseRowArray)
            {
                string tempItem = item;
                tempItem += ";";
                if (Regex.IsMatch(tempItem, pattern, RegexOptions.IgnoreCase))
                {
                    //we got it fam
                    databaseID = item.Substring(subStringStart, substringLength);
                    break;
                }
            }

            if (!databaseID.Equals(string.Empty))
            {
                string geturl = "http://rest.kegg.jp/get/" + databaseID + "//image";
                using (WebClient client = new WebClient())
                {
                    if (!File.Exists(@"c:\temp\" + databaseID + ".gif"))
                    {
                        if (!Directory.Exists(@"c:\temp\"))
                        {
                            //create dir
                            Directory.CreateDirectory(@"c:\temp\");
                        }
                        client.DownloadFile(new Uri(geturl), @"c:\temp\" + databaseID + ".gif");
                    }
                    displayImage.Source = new BitmapImage(new Uri(@"c:\temp\" + databaseID + ".gif", UriKind.RelativeOrAbsolute));
                }
            }
            else
            {
                //nothing found
                Uri notFoundImage;
                if (FunMode)
                {
                    notFoundImage = new Uri(@"data\notfound.jpg", UriKind.RelativeOrAbsolute);
                }
                else
                {
                    notFoundImage = new Uri(@"data\NotFound.png", UriKind.RelativeOrAbsolute);
                }
                displayImage.Source = new BitmapImage(notFoundImage);
            }

            Cursor = Cursors.Arrow;
            waitImageBorder.Visibility = Visibility.Hidden;
            simpleSound.Stop();

        }

        /// <summary>
        /// Moves the window when clicked
        /// </summary>
        /// <param name="sender">Sender</param>
        /// <param name="e">args</param>
        private void WindowMouseLeftButtonDown(object sender, MouseButtonEventArgs e) {
            DragMove();
        }

        /// <summary>
        /// Main search button for the program
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void Button_Click(object sender, RoutedEventArgs e) {
            SearchTerm st;
            if(pathwayField.Text != string.Empty) {
                st = SearchTerm.pathway;
            } else {
                st = SearchTerm.compound;
            }
            GetFromKegg(st);
            if (FunMode) {
                playSimpleSound();
                waitImageBorder.Visibility = Visibility.Visible;
            }
        }

        private void playSimpleSound() {
            simpleSound.Play();
        }

        public event PropertyChangedEventHandler PropertyChanged;

        private void NotifyPropertyChanged(String info) {
            if (PropertyChanged != null) {
                PropertyChanged(this, new PropertyChangedEventArgs(info));
            }
        }

        private void IsMuted_Checked(object sender, RoutedEventArgs e) {
            FunMode = true;
            
        }

        private void IsMuted_UnChecked(object sender, RoutedEventArgs e)
        {
            FunMode = false;
        }

        private enum SearchTerm
        {
            pathway, compound
        }

        private void loadPathways() {
            Pathways.Add("Glycolysis / Gluconeogenesis");
            Pathways.Add("Citrate cycle (TCA cycle)");
            Pathways.Add("Pentose phosphate pathway");
            Pathways.Add("Pentose and glucuronate interconversions");
            Pathways.Add("Fructose and mannose metabolism");
            Pathways.Add("Galactose metabolism");
            Pathways.Add("Ascorbate and aldarate metabolism");
            Pathways.Add("Fatty acid biosynthesis");
            Pathways.Add("Fatty acid elongation");
            Pathways.Add("Fatty acid degradation");
            Pathways.Add("Synthesis and degradation of ketone bodies");
            Pathways.Add("Cutin, suberine and wax biosynthesis");
            Pathways.Add("Steroid biosynthesis");
            Pathways.Add("Primary bile acid biosynthesis");
            Pathways.Add("Secondary bile acid biosynthesis");
            Pathways.Add("Ubiquinone and other terpenoid-quinone biosynthesis");
            Pathways.Add("Steroid hormone biosynthesis");
            Pathways.Add("Oxidative phosphorylation");
            Pathways.Add("Photosynthesis");
            Pathways.Add("Photosynthesis - antenna proteins");
            Pathways.Add("Arginine biosynthesis");
            Pathways.Add("Purine metabolism");
            Pathways.Add("Puromycin biosynthesis");
            Pathways.Add("Caffeine metabolism");
            Pathways.Add("Pyrimidine metabolism");
            Pathways.Add("Alanine, aspartate and glutamate metabolism");
            Pathways.Add("Tetracycline biosynthesis");
            Pathways.Add("Aflatoxin biosynthesis");
            Pathways.Add("Glycine, serine and threonine metabolism");
            Pathways.Add("Monobactam biosynthesis");
            Pathways.Add("Cysteine and methionine metabolism");
            Pathways.Add("Valine, leucine and isoleucine degradation");
            Pathways.Add("Geraniol degradation");
            Pathways.Add("Valine, leucine and isoleucine biosynthesis");
            Pathways.Add("Lysine biosynthesis");
            Pathways.Add("Lysine degradation");
            Pathways.Add("Penicillin and cephalosporin biosynthesis");
            Pathways.Add("Arginine and proline metabolism");
            Pathways.Add("Clavulanic acid biosynthesis");
            Pathways.Add("Carbapenem biosynthesis");
            Pathways.Add("Histidine metabolism");
            Pathways.Add("Tyrosine metabolism");
            Pathways.Add("DDT degradation");
            Pathways.Add("Phenylalanine metabolism");
            Pathways.Add("Chlorocyclohexane and chlorobenzene degradation");
            Pathways.Add("Benzoate degradation");
            Pathways.Add("Bisphenol degradation");
            Pathways.Add("Fluorobenzoate degradation");
            Pathways.Add("Furfural degradation");
            Pathways.Add("Tryptophan metabolism");
            Pathways.Add("Phenylalanine, tyrosine and tryptophan biosynthesis");
            Pathways.Add("Novobiocin biosynthesis");
            Pathways.Add("Benzoxazinoid biosynthesis");
            Pathways.Add("Indole diterpene alkaloid biosynthesis");
            Pathways.Add("Staurosporine biosynthesis");
            Pathways.Add("beta-Alanine metabolism");
            Pathways.Add("Taurine and hypotaurine metabolism");
            Pathways.Add("Phosphonate and phosphinate metabolism");
            Pathways.Add("Selenocompound metabolism");
            Pathways.Add("Cyanoamino acid metabolism");
            Pathways.Add("D-Glutamine and D-glutamate metabolism");
            Pathways.Add("D-Arginine and D-ornithine metabolism");
            Pathways.Add("D-Alanine metabolism");
            Pathways.Add("Glutathione metabolism");
            Pathways.Add("Starch and sucrose metabolism");
            Pathways.Add("N-Glycan biosynthesis");
            Pathways.Add("Other glycan degradation");
            Pathways.Add("Mucin type O-glycan biosynthesis");
            Pathways.Add("Various types of N-glycan biosynthesis");
            Pathways.Add("Other types of O-glycan biosynthesis");
            Pathways.Add("Mannose type O-glycan biosynthesis");
            Pathways.Add("Amino sugar and nucleotide sugar metabolism");
            Pathways.Add("Streptomycin biosynthesis");
            Pathways.Add("Biosynthesis of 12-, 14- and 16-membered macrolides");
            Pathways.Add("Polyketide sugar unit biosynthesis");
            Pathways.Add("Neomycin, kanamycin and gentamicin biosynthesis");
            Pathways.Add("Acarbose and validamycin biosynthesis");
            Pathways.Add("Glycosaminoglycan degradation");
            Pathways.Add("Glycosaminoglycan biosynthesis - chondroitin sulfate / dermatan sulfate");
            Pathways.Add("Glycosaminoglycan biosynthesis - keratan sulfate");
            Pathways.Add("Glycosaminoglycan biosynthesis - heparan sulfate / heparin");
            Pathways.Add("Lipopolysaccharide biosynthesis");
            Pathways.Add("Peptidoglycan biosynthesis");
            Pathways.Add("Glycerolipid metabolism");
            Pathways.Add("Inositol phosphate metabolism");
            Pathways.Add("Glycosylphosphatidylinositol (GPI)-anchor biosynthesis");
            Pathways.Add("Glycerophospholipid metabolism");
            Pathways.Add("Ether lipid metabolism");
            Pathways.Add("Arachidonic acid metabolism");
            Pathways.Add("Linoleic acid metabolism");
            Pathways.Add("alpha-Linolenic acid metabolism");
            Pathways.Add("Sphingolipid metabolism");
            Pathways.Add("Glycosphingolipid biosynthesis - lacto and neolacto series");
            Pathways.Add("Glycosphingolipid biosynthesis - globo and isoglobo series");
            Pathways.Add("Glycosphingolipid biosynthesis - ganglio series");
            Pathways.Add("Pyruvate metabolism");
            Pathways.Add("Dioxin degradation");
            Pathways.Add("Xylene degradation");
            Pathways.Add("Toluene degradation");
            Pathways.Add("Polycyclic aromatic hydrocarbon degradation");
            Pathways.Add("Chloroalkane and chloroalkene degradation");
            Pathways.Add("Naphthalene degradation");
            Pathways.Add("Aminobenzoate degradation");
            Pathways.Add("Glyoxylate and dicarboxylate metabolism");
            Pathways.Add("Nitrotoluene degradation");
            Pathways.Add("Propanoate metabolism");
            Pathways.Add("Ethylbenzene degradation");
            Pathways.Add("Styrene degradation");
            Pathways.Add("Butanoate metabolism");
            Pathways.Add("C5-Branched dibasic acid metabolism");
            Pathways.Add("One carbon pool by folate");
            Pathways.Add("Methane metabolism");
            Pathways.Add("Carbon fixation in photosynthetic organisms");
            Pathways.Add("Carbon fixation pathways in prokaryotes");
            Pathways.Add("Thiamine metabolism");
            Pathways.Add("Riboflavin metabolism");
            Pathways.Add("Vitamin B6 metabolism");
            Pathways.Add("Nicotinate and nicotinamide metabolism");
            Pathways.Add("Pantothenate and CoA biosynthesis");
            Pathways.Add("Biotin metabolism");
            Pathways.Add("Lipoic acid metabolism");
            Pathways.Add("Folate biosynthesis");
            Pathways.Add("Atrazine degradation");
            Pathways.Add("Retinol metabolism");
            Pathways.Add("Porphyrin and chlorophyll metabolism");
            Pathways.Add("Terpenoid backbone biosynthesis");
            Pathways.Add("Indole alkaloid biosynthesis");
            Pathways.Add("Monoterpenoid biosynthesis");
            Pathways.Add("Limonene and pinene degradation");
            Pathways.Add("Diterpenoid biosynthesis");
            Pathways.Add("Brassinosteroid biosynthesis");
            Pathways.Add("Carotenoid biosynthesis");
            Pathways.Add("Zeatin biosynthesis");
            Pathways.Add("Sesquiterpenoid and triterpenoid biosynthesis");
            Pathways.Add("Nitrogen metabolism");
            Pathways.Add("Sulfur metabolism");
            Pathways.Add("Caprolactam degradation");
            Pathways.Add("Phenylpropanoid biosynthesis");
            Pathways.Add("Flavonoid biosynthesis");
            Pathways.Add("Anthocyanin biosynthesis");
            Pathways.Add("Isoflavonoid biosynthesis");
            Pathways.Add("Flavone and flavonol biosynthesis");
            Pathways.Add("Stilbenoid, diarylheptanoid and gingerol biosynthesis");
            Pathways.Add("Isoquinoline alkaloid biosynthesis");
            Pathways.Add("Tropane, piperidine and pyridine alkaloid biosynthesis");
            Pathways.Add("Betalain biosynthesis");
            Pathways.Add("Glucosinolate biosynthesis");
            Pathways.Add("Aminoacyl-tRNA biosynthesis");
            Pathways.Add("Metabolism of xenobiotics by cytochrome P450");
            Pathways.Add("Insect hormone biosynthesis");
            Pathways.Add("Drug metabolism - cytochrome P450");
            Pathways.Add("Drug metabolism - other enzymes");
            Pathways.Add("Steroid degradation");
            Pathways.Add("Overview of biosynthetic pathways");
            Pathways.Add("Biosynthesis of unsaturated fatty acids");
            Pathways.Add("Biosynthesis of ansamycins");
            Pathways.Add("Type I polyketide structures");
            Pathways.Add("Biosynthesis of siderophore group nonribosomal peptides");
            Pathways.Add("Nonribosomal peptide structures");
            Pathways.Add("Biosynthesis of vancomycin group antibiotics");
            Pathways.Add("Biosynthesis of type II polyketide backbone");
            Pathways.Add("Biosynthesis of type II polyketide products");
            Pathways.Add("Acridone alkaloid biosynthesis");
            Pathways.Add("Biosynthesis of plant secondary metabolites");
            Pathways.Add("Biosynthesis of phenylpropanoids");
            Pathways.Add("Biosynthesis of terpenoids and steroids");
            Pathways.Add("Biosynthesis of alkaloids derived from shikimate pathway");
            Pathways.Add("Biosynthesis of alkaloids derived from ornithine, lysine and nicotinic acid");
            Pathways.Add("Biosynthesis of alkaloids derived from histidine and purine");
            Pathways.Add("Biosynthesis of alkaloids derived from terpenoid and polyketide");
            Pathways.Add("Biosynthesis of plant hormones");
            Pathways.Add("Metabolic pathways");
            Pathways.Add("Biosynthesis of secondary metabolites");
            Pathways.Add("Microbial metabolism in diverse environments");
            Pathways.Add("Biosynthesis of antibiotics");
            Pathways.Add("Carbon metabolism");
            Pathways.Add("2-Oxocarboxylic acid metabolism");
            Pathways.Add("Fatty acid metabolism");
            Pathways.Add("Degradation of aromatic compounds");
            Pathways.Add("Biosynthesis of amino acids");
            Pathways.Add("beta-Lactam resistance");
            Pathways.Add("Vancomycin resistance");
            Pathways.Add("Cationic antimicrobial peptide (CAMP) resistance");
            Pathways.Add("EGFR tyrosine kinase inhibitor resistance");
            Pathways.Add("Endocrine resistance");
            Pathways.Add("Antifolate resistance");
            Pathways.Add("Platinum drug resistance");
            Pathways.Add("ABC transporters");
            Pathways.Add("Two-component system");
            Pathways.Add("Quorum sensing");
            Pathways.Add("Biofilm formation - Pseudomonas aeruginosa");
            Pathways.Add("Bacterial chemotaxis");
            Pathways.Add("Flagellar assembly");
            Pathways.Add("Phosphotransferase system (PTS)");
            Pathways.Add("Ribosome biogenesis in eukaryotes");
            Pathways.Add("Ribosome");
            Pathways.Add("RNA transport");
            Pathways.Add("mRNA surveillance pathway");
            Pathways.Add("RNA degradation");
            Pathways.Add("RNA polymerase");
            Pathways.Add("Basal transcription factors");
            Pathways.Add("DNA replication");
            Pathways.Add("Spliceosome");
            Pathways.Add("Proteasome");
            Pathways.Add("Protein export");
            Pathways.Add("Bacterial secretion system");
            Pathways.Add("PPAR signaling pathway");
            Pathways.Add("Base excision repair");
            Pathways.Add("Nucleotide excision repair");
            Pathways.Add("Mismatch repair");
            Pathways.Add("Homologous recombination");
            Pathways.Add("Non-homologous end-joining");
            Pathways.Add("Fanconi anemia pathway");
            Pathways.Add("MAPK signaling pathway");
            Pathways.Add("MAPK signaling pathway - yeast");
            Pathways.Add("ErbB signaling pathway");
            Pathways.Add("MAPK signaling pathway - fly");
            Pathways.Add("Ras signaling pathway");
            Pathways.Add("Rap1 signaling pathway");
            Pathways.Add("MAPK signaling pathway - plant");
            Pathways.Add("Calcium signaling pathway");
            Pathways.Add("cGMP-PKG signaling pathway");
            Pathways.Add("cAMP signaling pathway");
            Pathways.Add("Cytokine-cytokine receptor interaction");
            Pathways.Add("Chemokine signaling pathway");
            Pathways.Add("NF-kappa B signaling pathway");
            Pathways.Add("HIF-1 signaling pathway");
            Pathways.Add("FoxO signaling pathway");
            Pathways.Add("Phosphatidylinositol signaling system");
            Pathways.Add("Sphingolipid signaling pathway");
            Pathways.Add("Phospholipase D signaling pathway");
            Pathways.Add("Plant hormone signal transduction");
            Pathways.Add("Neuroactive ligand-receptor interaction");
            Pathways.Add("Cell cycle");
            Pathways.Add("Cell cycle - yeast");
            Pathways.Add("Cell cycle - Caulobacter");
            Pathways.Add("Meiosis - yeast");
            Pathways.Add("Oocyte meiosis");
            Pathways.Add("p53 signaling pathway");
            Pathways.Add("Ubiquitin mediated proteolysis");
            Pathways.Add("Sulfur relay system");
            Pathways.Add("SNARE interactions in vesicular transport");
            Pathways.Add("Mitophagy - yeast");
            Pathways.Add("Autophagy");
            Pathways.Add("Protein processing in endoplasmic reticulum");
            Pathways.Add("Lysosome");
            Pathways.Add("Endocytosis");
            Pathways.Add("Phagosome");
            Pathways.Add("Peroxisome");
            Pathways.Add("mTOR signaling pathway");
            Pathways.Add("PI3K-Akt signaling pathway");
            Pathways.Add("AMPK signaling pathway");
            Pathways.Add("Apoptosis");
            Pathways.Add("Longevity regulating pathway");
            Pathways.Add("Longevity regulating pathway - worm");
            Pathways.Add("Longevity regulating pathway - multiple species");
            Pathways.Add("Apoptosis - fly");
            Pathways.Add("Apoptosis - multiple species");
            Pathways.Add("Cardiac muscle contraction");
            Pathways.Add("Adrenergic signaling in cardiomyocytes");
            Pathways.Add("Vascular smooth muscle contraction");
            Pathways.Add("Wnt signaling pathway");
            Pathways.Add("Dorso-ventral axis formation");
            Pathways.Add("Notch signaling pathway");
            Pathways.Add("Hedgehog signaling pathway");
            Pathways.Add("Hedgehog signaling pathway - fly");
            Pathways.Add("TGF-beta signaling pathway");
            Pathways.Add("Axon guidance");
            Pathways.Add("VEGF signaling pathway");
            Pathways.Add("Osteoclast differentiation");
            Pathways.Add("Hippo signaling pathway");
            Pathways.Add("Hippo signaling pathway - fly");
            Pathways.Add("Hippo signaling pathway -multiple species");
            Pathways.Add("Focal adhesion");
            Pathways.Add("ECM-receptor interaction");
            Pathways.Add("Cell adhesion molecules (CAMs)");
            Pathways.Add("Adherens junction");
            Pathways.Add("Tight junction");
            Pathways.Add("Gap junction");
            Pathways.Add("Signaling pathways regulating pluripotency of stem cells");
            Pathways.Add("Complement and coagulation cascades");
            Pathways.Add("Platelet activation");
            Pathways.Add("Antigen processing and presentation");
            Pathways.Add("Renin-angiotensin system");
            Pathways.Add("Toll-like receptor signaling pathway");
            Pathways.Add("NOD-like receptor signaling pathway");
            Pathways.Add("RIG-I-like receptor signaling pathway");
            Pathways.Add("Cytosolic DNA-sensing pathway");
            Pathways.Add("Toll and Imd signaling pathway");
            Pathways.Add("Plant-pathogen interaction");
            Pathways.Add("Jak-STAT signaling pathway");
            Pathways.Add("Hematopoietic cell lineage");
            Pathways.Add("Natural killer cell mediated cytotoxicity");
            Pathways.Add("Th1 and Th2 cell differentiation");
            Pathways.Add("T cell receptor signaling pathway");
            Pathways.Add("B cell receptor signaling pathway");
            Pathways.Add("Fc epsilon RI signaling pathway");
            Pathways.Add("Fc gamma R-mediated phagocytosis");
            Pathways.Add("TNF signaling pathway");
            Pathways.Add("Leukocyte transendothelial migration");
            Pathways.Add("Intestinal immune network for IgA production");
            Pathways.Add("Circadian rhythm");
            Pathways.Add("Circadian rhythm - fly");
            Pathways.Add("Circadian rhythm - plant");
            Pathways.Add("Circadian entrainment");
            Pathways.Add("Long-term potentiation");
            Pathways.Add("Synaptic vesicle cycle");
            Pathways.Add("Neurotrophin signaling pathway");
            Pathways.Add("Retrograde endocannabinoid signaling");
            Pathways.Add("Glutamatergic synapse");
            Pathways.Add("Cholinergic synapse");
            Pathways.Add("Serotonergic synapse");
            Pathways.Add("GABAergic synapse");
            Pathways.Add("Dopaminergic synapse");
            Pathways.Add("Long-term depression");
            Pathways.Add("Olfactory transduction");
            Pathways.Add("Taste transduction");
            Pathways.Add("Phototransduction");
            Pathways.Add("Phototransduction - fly");
            Pathways.Add("Inflammatory mediator regulation of TRP channels");
            Pathways.Add("Regulation of actin cytoskeleton");
            Pathways.Add("Insulin signaling pathway");
            Pathways.Add("Insulin secretion");
            Pathways.Add("GnRH signaling pathway");
            Pathways.Add("Ovarian steroidogenesis");
            Pathways.Add("Progesterone-mediated oocyte maturation");
            Pathways.Add("Estrogen signaling pathway");
            Pathways.Add("Melanogenesis");
            Pathways.Add("Prolactin signaling pathway");
            Pathways.Add("Thyroid hormone synthesis");
            Pathways.Add("Thyroid hormone signaling pathway");
            Pathways.Add("Adipocytokine signaling pathway");
            Pathways.Add("Oxytocin signaling pathway");
            Pathways.Add("Glucagon signaling pathway");
            Pathways.Add("Regulation of lipolysis in adipocytes");
            Pathways.Add("Renin secretion");
            Pathways.Add("Aldosterone synthesis and secretion");
            Pathways.Add("Type II diabetes mellitus");
            Pathways.Add("Insulin resistance");
            Pathways.Add("Non-alcoholic fatty liver disease (NAFLD)");
            Pathways.Add("AGE-RAGE signaling pathway in diabetic complications");
            Pathways.Add("Type I diabetes mellitus");
            Pathways.Add("Maturity onset diabetes of the young");
            Pathways.Add("Aldosterone-regulated sodium reabsorption");
            Pathways.Add("Endocrine and other factor-regulated calcium reabsorption");
            Pathways.Add("Vasopressin-regulated water reabsorption");
            Pathways.Add("Proximal tubule bicarbonate reclamation");
            Pathways.Add("Collecting duct acid secretion");
            Pathways.Add("Salivary secretion");
            Pathways.Add("Gastric acid secretion");
            Pathways.Add("Pancreatic secretion");
            Pathways.Add("Carbohydrate digestion and absorption");
            Pathways.Add("Protein digestion and absorption");
            Pathways.Add("Fat digestion and absorption");
            Pathways.Add("Bile secretion");
            Pathways.Add("Vitamin digestion and absorption");
            Pathways.Add("Mineral absorption");
            Pathways.Add("Alzheimer's disease");
            Pathways.Add("Parkinson's disease");
            Pathways.Add("Amyotrophic lateral sclerosis (ALS)");
            Pathways.Add("Huntington's disease");
            Pathways.Add("Prion diseases");
            Pathways.Add("Cocaine addiction");
            Pathways.Add("Amphetamine addiction");
            Pathways.Add("Morphine addiction");
            Pathways.Add("Nicotine addiction");
            Pathways.Add("Alcoholism");
            Pathways.Add("Bacterial invasion of epithelial cells");
            Pathways.Add("Vibrio cholerae infection");
            Pathways.Add("Biofilm formation - Vibrio cholerae");
            Pathways.Add("Epithelial cell signaling in Helicobacter pylori infection");
            Pathways.Add("Pathogenic Escherichia coli infection");
            Pathways.Add("Shigellosis");
            Pathways.Add("Salmonella infection");
            Pathways.Add("Pertussis");
            Pathways.Add("Legionellosis");
            Pathways.Add("Leishmaniasis");
            Pathways.Add("Chagas disease (American trypanosomiasis)");
            Pathways.Add("African trypanosomiasis");
            Pathways.Add("Malaria");
            Pathways.Add("Toxoplasmosis");
            Pathways.Add("Amoebiasis");
            Pathways.Add("Staphylococcus aureus infection");
            Pathways.Add("Tuberculosis");
            Pathways.Add("Hepatitis C");
            Pathways.Add("Hepatitis B");
            Pathways.Add("Measles");
            Pathways.Add("Influenza A");
            Pathways.Add("HTLV-I infection");
            Pathways.Add("Herpes simplex infection");
            Pathways.Add("Epstein-Barr virus infection");
            Pathways.Add("Pathways in cancer");
            Pathways.Add("Transcriptional misregulation in cancer");
            Pathways.Add("Viral carcinogenesis");
            Pathways.Add("Chemical carcinogenesis");
            Pathways.Add("Proteoglycans in cancer");
            Pathways.Add("MicroRNAs in cancer");
            Pathways.Add("Colorectal cancer");
            Pathways.Add("Renal cell carcinoma");
            Pathways.Add("Pancreatic cancer");
            Pathways.Add("Endometrial cancer");
            Pathways.Add("Glioma");
            Pathways.Add("Prostate cancer");
            Pathways.Add("Thyroid cancer");
            Pathways.Add("Basal cell carcinoma");
            Pathways.Add("Melanoma");
            Pathways.Add("Bladder cancer");
            Pathways.Add("Chronic myeloid leukemia");
            Pathways.Add("Acute myeloid leukemia");
            Pathways.Add("Small cell lung cancer");
            Pathways.Add("Non-small cell lung cancer");
            Pathways.Add("Breast cancer");
            Pathways.Add("Central carbon metabolism in cancer");
            Pathways.Add("Choline metabolism in cancer");
            Pathways.Add("Asthma");
            Pathways.Add("Autoimmune thyroid disease");
            Pathways.Add("Inflammatory bowel disease (IBD)");
            Pathways.Add("Systemic lupus erythematosus");
            Pathways.Add("Rheumatoid arthritis");
            Pathways.Add("Allograft rejection");
            Pathways.Add("Graft-versus-host disease");
            Pathways.Add("Primary immunodeficiency");
            Pathways.Add("Hypertrophic cardiomyopathy (HCM)");
            Pathways.Add("Arrhythmogenic right ventricular cardiomyopathy (ARVC)");
            Pathways.Add("Dilated cardiomyopathy");
            Pathways.Add("Viral myocarditis");
            Pathways.Add("Penicillins");
            Pathways.Add("Cephalosporins - parenteral agents");
            Pathways.Add("Cephalosporins - oral agents");
            Pathways.Add("Quinolones");
            Pathways.Add("Local analgesics");
            Pathways.Add("Sulfonamide derivatives - sulfa drugs");
            Pathways.Add("Sulfonamide derivatives - diuretics");
            Pathways.Add("Sulfonamide derivatives - hypoglycemic agents");
            Pathways.Add("Tetracyclines");
            Pathways.Add("Macrolides and ketolides");
            Pathways.Add("Aminoglycosides");
            Pathways.Add("Rifamycins");
            Pathways.Add("HMG-CoA reductase inhibitors");
            Pathways.Add("Quinolines");
            Pathways.Add("Antifungal agents");
            Pathways.Add("Antidepressants");
            Pathways.Add("Antipsychotics");
            Pathways.Add("Phenothiazines");
            Pathways.Add("Anxiolytics");
            Pathways.Add("Butyrophenones");
            Pathways.Add("Hypnotics");
            Pathways.Add("Anticonvulsants");
            Pathways.Add("Eicosanoids");
            Pathways.Add("Prostaglandins");
            Pathways.Add("Calcium channel blocking drugs");
            Pathways.Add("Antiarrhythmic drugs");
            Pathways.Add("Antiulcer drugs");
            Pathways.Add("Opioid analgesics");
            Pathways.Add("Antineoplastics - alkylating agents");
            Pathways.Add("Antineoplastics - antimetabolic agents");
            Pathways.Add("Antineoplastics - agents from natural products");
            Pathways.Add("Antineoplastics - hormones");
            Pathways.Add("Antiviral agents");
            Pathways.Add("Antineoplastics - protein kinases inhibitors");
            Pathways.Add("Immunosuppressive agents");
            Pathways.Add("Osteoporosis drugs");
            Pathways.Add("Antimigraines");
            Pathways.Add("Antithrombosis agents");
            Pathways.Add("Antirheumatics - DMARDs and biological agents");
            Pathways.Add("Antidiabetics");
            Pathways.Add("Antidyslipidemic agents");
            Pathways.Add("Anti-HIV agents");
            Pathways.Add("Antiglaucoma agents");
            Pathways.Add("Sulfonamide derivatives - overview");
            Pathways.Add("Agents for Alzheimer-type dementia");
            Pathways.Add("Antiparkinsonian agents");
            Pathways.Add("Benzoic acid family");
            Pathways.Add("1,2-Diphenyl substitution family");
            Pathways.Add("Naphthalene family");
            Pathways.Add("Benzodiazepine family");
            Pathways.Add("Serotonin receptor agonists/antagonists");
            Pathways.Add("Histamine H1 receptor antagonists");
            Pathways.Add("Dopamine receptor agonists/antagonists");
            Pathways.Add("beta-Adrenergic receptor agonists/antagonists");
            Pathways.Add("alpha-Adrenergic receptor agonists/antagonists");
            Pathways.Add("Catecholamine transferase inhibitors");
            Pathways.Add("Renin-angiotensin system inhibitors");
            Pathways.Add("HIV protease inhibitors");
            Pathways.Add("Cyclooxygenase inhibitors");
            Pathways.Add("Cholinergic and anticholinergic drugs");
            Pathways.Add("Nicotinic cholinergic receptor antagonists");
            Pathways.Add("Peroxisome proliferator-activated receptor (PPAR) agonists");
            Pathways.Add("Retinoic acid receptor (RAR) and retinoid X receptor (RXR) agonists/antagonists");
            Pathways.Add("Opioid receptor agonists/antagonists");
            Pathways.Add("Glucocorticoid and meneralocorticoid receptor agonists/antagonists");
            Pathways.Add("Progesterone, androgen and estrogen receptor agonists/antagonists");
            Pathways.Add("Histamine H2/H3 receptor agonists/antagonists");
            Pathways.Add("Eicosanoid receptor agonists/antagonists");
            Pathways.Add("Angiotensin receptor and endothelin receptor antagonists");
            Pathways.Add("GABA-A receptor agonists/antagonists");
            Pathways.Add("Sodium channel blocking drugs");
            Pathways.Add("Potassium channel blocking and opening drugs");
            Pathways.Add("Ion transporter inhibitors");
            Pathways.Add("Neurotransmitter transporter inhibitors");
            Pathways.Add("N-Metyl-D-aspartic acid receptor antagonists");

        }


    }
}
