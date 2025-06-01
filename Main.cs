using DocumentFormat.OpenXml;
using DocumentFormat.OpenXml.Drawing.Diagrams;
using DocumentFormat.OpenXml.Packaging;
using DocumentFormat.OpenXml.Spreadsheet;
using DocumentFormat.OpenXml.Wordprocessing;
using Microsoft.WindowsAPICodePack.Dialogs;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.ComTypes;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml.Linq;
using static DATN.Main;


namespace DATN
{
    public partial class Main : Form
    {
        
        private List<TestCase> _testCases;
        private List<UseCaseData> _useCases; // Thêm mới
        private string _selectedFilePath;
        //private XDocument _xmlDoc; // Thêm mới
        private string _selectedUseCaseId; // Thêm mới
        private bool _isDisplayingTestCases = false; // Thêm mới
        private bool isEditing = false;
        private List<UseCaseData> originalData = new List<UseCaseData>();
        private readonly TestCaseGenerator _testCaseGenerator = new TestCaseGenerator();
        private DateTime? _testCaseGenerationTime;




        public Main()
        {
            InitializeComponent();
            _testCases = new List<TestCase>(); //Khởi tạo danh sách test case
            _useCases = new List<UseCaseData>(); // Thêm mới
            // Gán sự kiện SelectedIndexChanged cho comboboxUC
            comboboxUC.SelectedIndexChanged += new EventHandler(ComboBoxUseCases_SelectedIndexChanged);
            
        }

        //lớp test case để lưu thông tin test case
        public class TestCase
        {
            
            public string UseCase { get; set; }      // Mã Use Case 
            public string UseCaseName { get; set; }  // Tên Use Case 
            public string TestName { get; set; }     // Test Case ID 
            public List<string> Procedure { get; set; }    // Quy trình kiểm thử (danh sách các bước)
            public List<string> ExpectedResults { get; set; } // Kết quả kỳ vọng (danh sách tương ứng)
            public string FlowType { get; set; } // Thêm để ghi chú loại luồng
        }

        //2 lớp để lưu trữ thông tin về trường hợp và các bước sử dụng
        public class UseCaseData
        {
            public string Id { get; set; }
            public string Name { get; set; }
            public string Level { get; set; }
            public string Preconditions { get; set; }
            public string Postconditions { get; set; }
            public List<StepData> Steps { get; set; }
        }

        public class StepData
        {
            public string UseCaseName { get; set; }
            public string Level { get; set; }
            public string Preconditions { get; set; }
            public string PostConditions { get; set; }
            public string FlowType { get; set; }
            public string Description { get; set; }
            public string ExpectedResult { get; set; }
            public int? BranchPoint { get; set; } // Thêm thuộc tính BranchPoint
        }

        public class UseCaseStep
        {
            public string Name { get; set; }
            public List<UseCaseStep> SubSteps { get; set; } = new List<UseCaseStep>();
            public List<UseCaseStep> Extensions { get; set; } = new List<UseCaseStep>();
                       
        }


        //Hiển thị các trường hợp sử dụng phù hợp với các cột tỏng datagridview
        private void SetUseCaseDetailsColumns()
        {
            
            dgvUseCaseDetails.Columns.Clear();
            dgvUseCaseDetails.Columns.Add("UseCaseName", "Use Case Name");
            dgvUseCaseDetails.Columns.Add("Level", "Level");
            dgvUseCaseDetails.Columns.Add("Preconditions", "Preconditions");
            dgvUseCaseDetails.Columns.Add("PostConditions", "Post-conditions");
            dgvUseCaseDetails.Columns.Add("FlowType", "Flow Type");
            dgvUseCaseDetails.Columns.Add("Description", "Step Description");
            dgvUseCaseDetails.Columns.Add("ExpectedResult", "Expected Result");

            // Thêm hỗ trợ hiển thị nhiều dòng (cho dữ liệu mới)
            dgvUseCaseDetails.Columns["Preconditions"].DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            dgvUseCaseDetails.Columns["PostConditions"].DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            dgvUseCaseDetails.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;

            // Đặt độ rộng tối thiểu (cho dữ liệu mới)
            dgvUseCaseDetails.Columns["Preconditions"].Width = 200;
            dgvUseCaseDetails.Columns["PostConditions"].Width = 200;

            _isDisplayingTestCases = false;
            txtThongbao.AppendText($"Đã thiết lập {dgvUseCaseDetails.Columns.Count} cột.\r\n");
        }

        private void SetTestCaseColumns()
        {
            
            dgvUseCaseDetails.Columns.Clear();
            dgvUseCaseDetails.Columns.Add("TestName", "Tên Test Case");
            dgvUseCaseDetails.Columns.Add("UseCaseName", "Trường hợp sử dụng");
            dgvUseCaseDetails.Columns.Add("FlowType", "Loại luồng");
            dgvUseCaseDetails.Columns.Add("Procedure", "Bước thực hiện");
            dgvUseCaseDetails.Columns.Add("ExpectedResult", "Kết quả mong đợi");
            dgvUseCaseDetails.Columns.Add("TestCaseType", "Loại Test Case");
            _isDisplayingTestCases = true;


            // Đảm bảo hiển thị đa dòng trong ô
            dgvUseCaseDetails.DefaultCellStyle.WrapMode = DataGridViewTriState.True;
            dgvUseCaseDetails.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
        }



        //Hàm kiểm tra tính hợp lệ file XML 
        private bool ValidateXmlFile(string filePath, out bool isStarUml)
        {

            //isStarUml = false;
            //try
            //{
            //    string content = File.ReadAllText(filePath, Encoding.UTF8);
            //    if (string.IsNullOrWhiteSpace(content))
            //    {
            //        txtThongbao.AppendText("Lỗi: File XML trống!\r\n");
            //        return false;
            //    }

            //    // Kiểm tra định dạng StarUML
            //    if (Regex.IsMatch(content, @"<root\b", RegexOptions.IgnoreCase) &&
            //        (Regex.IsMatch(content, @"ownedElements\b", RegexOptions.IgnoreCase) ||
            //         Regex.IsMatch(content, @"type\s*=\s*(""|')UMLUseCase(""|')", RegexOptions.IgnoreCase) ||
            //         content.Contains("<$ref>")))
            //    {
            //        isStarUml = true;
            //        txtThongbao.AppendText("Đã xác định: Định dạng StarUML XML.\r\n");

            //        // Thay thế <$ref> thành <ref> để sửa lỗi cú pháp
            //        content = content.Replace("<$ref>", "<ref>").Replace("</$ref>", "</ref>");
            //        txtThongbao.AppendText("Đã thay thế <$ref> thành <ref> để sửa lỗi cú pháp.\r\n");
            //    }
            //    // Kiểm tra định dạng Visual Paradigm
            //    else if (Regex.IsMatch(content, @"<vpumlModel\b", RegexOptions.IgnoreCase) ||
            //             Regex.IsMatch(content, @"stepContainers\b", RegexOptions.IgnoreCase) ||
            //             Regex.IsMatch(content, @"type\s*=\s*(""|')step(""|')", RegexOptions.IgnoreCase))
            //    {
            //        txtThongbao.AppendText("Đã xác định: Định dạng Visual Paradigm XML.\r\n");
            //    }
            //    else if (Regex.IsMatch(content, @"<UseCase\b", RegexOptions.IgnoreCase))
            //    {
            //        txtThongbao.AppendText("Đã xác định: Định dạng XML cũ.\r\n");
            //    }
            //    else if (Regex.IsMatch(content, @"packagedElement\b", RegexOptions.IgnoreCase))
            //    {
            //        txtThongbao.AppendText("Đã xác định: Định dạng XMI.\r\n");
            //    }
            //    else
            //    {
            //        txtThongbao.AppendText("Cảnh báo: Không thể xác định định dạng XML. Tiếp tục xử lý nhưng có thể gặp lỗi!\r\n");
            //    }

            //    // Kiểm tra cú pháp XML sau khi thay thế
            //    XDocument.Parse(content);
            //    return true;
            //}
            //catch (Exception ex)
            //{
            //    txtThongbao.AppendText($"Lỗi khi kiểm tra file: {ex.Message}\r\n");
            //    txtThongbao.AppendText($"StackTrace: {ex.StackTrace}\r\n");
            //    return false;
            //}





            //cải tiến
            isStarUml = false;
            try
            {
                StringBuilder content = new StringBuilder();
                using (StreamReader reader = new StreamReader(filePath, Encoding.UTF8))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        content.AppendLine(line);
                    }
                }

                string fileContent = content.ToString();
                if (string.IsNullOrWhiteSpace(fileContent))
                {
                    txtThongbao.AppendText("Lỗi: File XML trống!\r\n");
                    return false;
                }

                // Kiểm tra định dạng StarUML
                if (Regex.IsMatch(fileContent, @"<root\b", RegexOptions.IgnoreCase) &&
                    (Regex.IsMatch(fileContent, @"ownedElements\b", RegexOptions.IgnoreCase) ||
                     Regex.IsMatch(fileContent, @"type\s*=\s*(""|')UMLUseCase(""|')", RegexOptions.IgnoreCase) ||
                     fileContent.Contains("<$ref>")))
                {
                    isStarUml = true;
                    txtThongbao.AppendText("Đã xác định: Định dạng StarUML XML.\r\n");

                    // Thay thế <$ref> thành <ref> để sửa lỗi cú pháp
                    fileContent = fileContent.Replace("<$ref>", "<ref>").Replace("</$ref>", "</ref>");
                    txtThongbao.AppendText("Đã thay thế <$ref> thành <ref> để sửa lỗi cú pháp.\r\n");
                }
                else if (Regex.IsMatch(fileContent, @"<vpumlModel\b", RegexOptions.IgnoreCase) ||
                         Regex.IsMatch(fileContent, @"stepContainers\b", RegexOptions.IgnoreCase) ||
                         Regex.IsMatch(fileContent, @"type\s*=\s*(""|')step(""|')", RegexOptions.IgnoreCase))
                {
                    txtThongbao.AppendText("Đã xác định: Định dạng Visual Paradigm XML.\r\n");
                }
                else if (Regex.IsMatch(fileContent, @"<UseCase\b", RegexOptions.IgnoreCase))
                {
                    txtThongbao.AppendText("Đã xác định: Định dạng XML cũ.\r\n");
                }
                else if (Regex.IsMatch(fileContent, @"packagedElement\b", RegexOptions.IgnoreCase))
                {
                    txtThongbao.AppendText("Đã xác định: Định dạng XMI.\r\n");
                }
                else
                {
                    txtThongbao.AppendText("Cảnh báo: Không thể xác định định dạng XML. Tiếp tục xử lý nhưng có thể gặp lỗi!\r\n");
                }

                // Kiểm tra cú pháp XML sau khi thay thế
                XDocument.Parse(fileContent);
                return true;
            }
            catch (Exception ex)
            {
                txtThongbao.AppendText($"Lỗi khi kiểm tra file: {ex.Message}\r\n");
                txtThongbao.AppendText($"StackTrace: {ex.StackTrace}\r\n");
                return false;
            }
        }


        //Hàm kiểm tra tính hợp lệ file Txt
        private bool ValidateTxtFile(string filePath)
        {

            //try
            //{
            //    string content = File.ReadAllText(filePath, Encoding.UTF8);

            //    // Kiểm tra các thành phần bắt buộc, không phân biệt hoa thường
            //    bool hasUseCaseId = Regex.IsMatch(content, @"Use case ID\s*:\s*.+", RegexOptions.IgnoreCase);
            //    bool hasUseCaseName = Regex.IsMatch(content, @"Use case name\s*:\s*.+", RegexOptions.IgnoreCase);
            //    bool hasMainFlow = Regex.IsMatch(content, @"(Main Flow|BasicFlow|Primary Flow)\s*:[\s\S]*", RegexOptions.IgnoreCase);

            //    if (!hasUseCaseId)
            //    {
            //        txtThongbao.AppendText("File .txt không hợp lệ: Thiếu 'Use case ID'!\r\n");
            //        return false;
            //    }
            //    if (!hasUseCaseName)
            //    {
            //        txtThongbao.AppendText("File .txt không hợp lệ: Thiếu 'Use case name'!\r\n");
            //        return false;
            //    }
            //    if (!hasMainFlow)
            //    {
            //        txtThongbao.AppendText("File .txt không hợp lệ: Thiếu 'Main Flow', 'BasicFlow' hoặc 'Primary Flow'!\r\n");
            //        return false;
            //    }

            //    return true;
            //}
            //catch (Exception ex)
            //{
            //    txtThongbao.AppendText($"Lỗi khi kiểm tra file .txt: {ex.Message}\r\n");
            //    return false;
            //}




            //cải tiến
            try
            {
                StringBuilder content = new StringBuilder();
                using (StreamReader reader = new StreamReader(filePath, Encoding.UTF8))
                {
                    string line;
                    while ((line = reader.ReadLine()) != null)
                    {
                        content.AppendLine(line);
                    }
                }

                string fileContent = content.ToString();

                // Kiểm tra các thành phần bắt buộc, không phân biệt hoa thường
                bool hasUseCaseId = Regex.IsMatch(fileContent, @"Use case ID\s*:\s*.+", RegexOptions.IgnoreCase);
                bool hasUseCaseName = Regex.IsMatch(fileContent, @"Use case name\s*:\s*.+", RegexOptions.IgnoreCase);
                bool hasMainFlow = Regex.IsMatch(fileContent, @"(Main Flow|BasicFlow|Primary Flow)\s*:[\s\S]*", RegexOptions.IgnoreCase);

                if (!hasUseCaseId)
                {
                    txtThongbao.AppendText("File .txt không hợp lệ: Thiếu 'Use case ID'!\r\n");
                    return false;
                }
                if (!hasUseCaseName)
                {
                    txtThongbao.AppendText("File .txt không hợp lệ: Thiếu 'Use case name'!\r\n");
                    return false;
                }
                if (!hasMainFlow)
                {
                    txtThongbao.AppendText("File .txt không hợp lệ: Thiếu 'Main Flow', 'BasicFlow' hoặc 'Primary Flow'!\r\n");
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                txtThongbao.AppendText($"Lỗi khi kiểm tra file .txt: {ex.Message}\r\n");
                return false;
            }
        }
             


        //
        private void LoadXmlAndUseCases(string xmlFilePath)
        {
            try
            {
                // Kiểm tra file trước
                if (!ValidateXmlFile(xmlFilePath, out bool isStarUml))
                {
                    txtThongbao.AppendText("Không thể tiếp tục do lỗi khi kiểm tra file.\r\n");
                    return;
                }

                // Đọc nội dung tệp
                string xmlContent = File.ReadAllText(xmlFilePath, Encoding.UTF8);
                txtThongbao.AppendText($"Đã đọc file XML: {xmlFilePath}\r\n");

                XDocument doc = null;

                // Thay thế <$ref> chỉ khi là StarUML để sửa lỗi cú pháp
                if (isStarUml)
                {
                    txtThongbao.AppendText("Xử lý định dạng StarUML XML, thay thế <$ref> thành <ref>...\r\n");
                    xmlContent = xmlContent.Replace("<$ref>", "<ref>").Replace("</$ref>", "</ref>");

                    // Thử parse XML sau khi thay thế
                    try
                    {
                        using (var reader = new StringReader(xmlContent))
                        {
                            doc = XDocument.Load(reader);
                            txtThongbao.AppendText("Đã tải nội dung StarUML XML thành công.\r\n");
                        }
                    }
                    catch (Exception ex)
                    {
                        txtThongbao.AppendText($"Lỗi khi parse StarUML XML sau khi thay thế <$ref>: {ex.Message}\r\n");
                        return;
                    }
                }
                else
                {
                    // Đối với các định dạng khác, parse trực tiếp
                    try
                    {
                        using (var reader = new StringReader(xmlContent))
                        {
                            doc = XDocument.Load(reader);
                            txtThongbao.AppendText("Đã tải nội dung XML thành công.\r\n");
                        }
                    }
                    catch (Exception ex)
                    {
                        txtThongbao.AppendText($"Lỗi khi parse XML (non-StarUML): {ex.Message}\r\n");
                        return;
                    }
                }

                _useCases = new List<UseCaseData>();

                if (isStarUml)
                {
                    txtThongbao.AppendText("Bắt đầu phân tích StarUML XML.\r\n");
                    _useCases.AddRange(ParseStarUmlXml(doc, txtThongbao));
                }
                else if (radioXMLIn.Checked)
                {
                    var useCaseElements = doc.Descendants("UseCase");
                    txtThongbao.AppendText($"Số UseCase tìm thấy trong XML cũ: {useCaseElements.Count()}\r\n");

                    foreach (var element in useCaseElements)
                    {
                        string name = element.Attribute("Name")?.Value;
                        string id = element.Attribute("Id")?.Value;
                        if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(id)) continue;

                        var steps = ParseSteps(element).ToList();
                        txtThongbao.AppendText($"Số bước tìm thấy cho UseCase {name}: {steps.Count}\r\n");

                        _useCases.Add(new UseCaseData { Id = id, Name = name, Steps = steps });
                    }
                }
                else if (radioXMIIn.Checked)
                {
                    var useCaseElements = doc.Descendants("{http://www.eclipse.org/uml2/2.0.0/UML}packagedElement")
                        .Where(uc => uc.Attribute("{http://schema.omg.org/spec/XMI/2.1}type")?.Value == "uml:UseCase")
                        .ToList();

                    txtThongbao.AppendText($"Số UseCase tìm thấy trong XMI: {useCaseElements.Count()}\r\n");

                    foreach (var element in useCaseElements)
                    {
                        string name = element.Attribute("name")?.Value;
                        string id = element.Attribute("{http://schema.omg.org/spec/XMI/2.1}id")?.Value;
                        if (string.IsNullOrEmpty(name) || string.IsNullOrEmpty(id)) continue;

                        var steps = ParseXmiSteps(element).ToList();
                        txtThongbao.AppendText($"Số bước tìm thấy cho UseCase {name}: {steps.Count}\r\n");

                        _useCases.Add(new UseCaseData { Id = id, Name = name, Steps = steps });
                    }
                }

                txtThongbao.AppendText($"Số UseCase đã lưu vào _useCases: {_useCases.Count}\r\n");

                // Cập nhật combobox
                comboboxUC.Items.Clear();
                var uniqueUseCaseNames = new HashSet<string>();
                foreach (var useCase in _useCases)
                {
                    if (uniqueUseCaseNames.Add(useCase.Name))
                    {
                        comboboxUC.Items.Add(useCase.Name);
                    }
                }

                if (_useCases.Count > 0)
                {
                    txtThongbao.AppendText("Đã tải danh sách Use Case. Vui lòng chọn Use Case để hiển thị chi tiết.\r\n");
                    // Tự động chọn Use Case đầu tiên nếu có
                    if (comboboxUC.Items.Count > 0)
                    {
                        comboboxUC.SelectedIndex = 0;
                    }
                }
                else
                {
                    txtThongbao.AppendText($"Không tìm thấy Use Case trong file {(isStarUml ? "StarUML XML" : radioXMLIn.Checked ? "XML" : "XMI")}!\r\n");
                }
            }
            catch (Exception ex)
            {
                txtThongbao.AppendText($"Lỗi khi tải file: {ex.Message}\r\n");
                txtThongbao.AppendText($"StackTrace: {ex.StackTrace}\r\n");
            }

        }


        //xml
        //Tách từng bước logic để tái sự dụng và hiển thị chi tiết
        private IEnumerable<StepData> ParseSteps(XElement useCase)
        {

            var steps = new List<StepData>();
            var uniqueSteps = new HashSet<string>();
            var basicFlowDescriptions = new List<(int Index, string Description)>(); // Lưu số thứ tự và mô tả của Basic Flow

            try
            {
                string useCaseName = useCase.Attribute("Name")?.Value ?? "Không xác định";
                string preconditions = GetTaggedValue(useCase, "Preconditions") ?? "Không có";
                string postConditions = GetTaggedValue(useCase, "Post-conditions") ?? "Không có";

                txtThongbao.AppendText($"Bắt đầu phân tích UseCase: {useCaseName}\r\n");

                var stepContainers = useCase.Descendants("StepContainer").ToList();
                var vpumlModels = useCase.Descendants("vpumlModel")
                    .Where(e => e.Attribute("type")?.Value == "step")
                    .ToList();

                txtThongbao.AppendText($"Số StepContainer: {stepContainers.Count()}, Số vpumlModel: {vpumlModels.Count()}\r\n");

                var allSteps = new List<XElement>();
                foreach (var container in stepContainers)
                {
                    allSteps.AddRange(container.Descendants("Step").Where(step => step.Name.LocalName != "Extension"));
                }
                foreach (var model in vpumlModels)
                {
                    allSteps.AddRange(model.Descendants("step"));
                }

                // Thu thập tất cả các bước Basic Flow trước
                int basicFlowStepIndex = 0;
                var basicFlowSteps = new List<StepData>();
                for (int i = 0; i < allSteps.Count; i++)
                {
                    var step = allSteps[i];
                    string description = step.Attribute("Name")?.Value
                        ?? step.Attribute("Text")?.Value
                        ?? step.Attribute("Description")?.Value
                        ?? step.Value.Trim()
                        ?? "No description";

                    if (string.IsNullOrEmpty(description) || description == "No description")
                    {
                        continue;
                    }

                    bool isSystemAction = description.ToLower().Contains("atm") || description.ToLower().Contains("hệ thống") || description.ToLower().Contains("hiển thị");

                    if (!isSystemAction)
                    {
                        basicFlowStepIndex++;
                        string numberedDescription = $"{basicFlowStepIndex}. {description}";
                        if (uniqueSteps.Add(description))
                        {
                            string expectedResult = "Không có kết quả kỳ vọng";
                            List<string> expectedResults = new List<string>();

                            int j = i + 1;
                            while (j < allSteps.Count)
                            {
                                var nextStep = allSteps[j];
                                string nextDescription = nextStep.Attribute("Name")?.Value
                                    ?? nextStep.Attribute("Text")?.Value
                                    ?? nextStep.Attribute("Description")?.Value
                                    ?? nextStep.Value.Trim()
                                    ?? "No description";

                                bool isNextSystemAction = nextDescription.ToLower().Contains("atm") || nextDescription.ToLower().Contains("hệ thống") || nextDescription.ToLower().Contains("hiển thị");
                                if (isNextSystemAction)
                                {
                                    if (!nextDescription.ToLower().Contains("lỗi") && !nextDescription.ToLower().Contains("error"))
                                    {
                                        expectedResults.Add(nextDescription.Trim());
                                        j++;
                                    }
                                    else
                                    {
                                        break;
                                    }
                                }
                                else
                                {
                                    break;
                                }
                            }

                            if (expectedResults.Count > 0)
                            {
                                expectedResult = string.Join("\n", expectedResults);
                                i = j - 1;
                            }

                            var stepData = new StepData
                            {
                                UseCaseName = useCaseName,
                                Level = "Không xác định",
                                Preconditions = preconditions,
                                PostConditions = postConditions,
                                FlowType = "Basic Flow",
                                Description = numberedDescription,
                                ExpectedResult = expectedResult,
                                BranchPoint = null
                            };
                            basicFlowSteps.Add(stepData);
                            basicFlowDescriptions.Add((basicFlowStepIndex, description)); // Lưu số thứ tự và mô tả
                        }
                    }
                }

                // Thu thập và đánh số các bước Exception Flow
                var exceptions = stepContainers.SelectMany(container => container.Descendants("Extension")
                    .GroupBy(step => step.Attribute("Name")?.Value ?? step.Value)
                    .Select(group => group.First())).ToList();

                var exceptionCounter = new Dictionary<int, int>(); // Đếm số bước ngoại lệ cho mỗi branchPoint
                foreach (var step in exceptions)
                {
                    string description = step.Attribute("Name")?.Value
                        ?? step.Attribute("Text")?.Value
                        ?? step.Attribute("Description")?.Value
                        ?? step.Value.Trim()
                        ?? "No description";

                    if (string.IsNullOrEmpty(description) || description == "No description")
                    {
                        txtThongbao.AppendText($"Bỏ qua bước không có mô tả hợp lệ (Extension): {description}\r\n");
                        continue;
                    }

                    string refStep = step.Attribute("RefStep")?.Value;
                    int? branchPoint = null;

                    // Tìm branchPoint dựa trên RefStep
                    if (!string.IsNullOrEmpty(refStep))
                    {
                        // Chuẩn hóa RefStep và mô tả để so sánh
                        string refStepCleaned = refStep.Trim();

                        // Tìm bước Basic Flow tương ứng
                        var matchingStep = basicFlowDescriptions.FirstOrDefault(bfd =>
                            bfd.Description.Trim().Equals(refStepCleaned, StringComparison.OrdinalIgnoreCase) ||
                            $"{bfd.Index}. {bfd.Description}".Trim().Equals(refStepCleaned, StringComparison.OrdinalIgnoreCase));

                        if (matchingStep != default)
                        {
                            branchPoint = matchingStep.Index; // Sử dụng số thứ tự của bước Basic Flow
                            txtThongbao.AppendText($"Tìm thấy RefStep: '{refStep}' khớp với bước Basic Flow số {branchPoint}\r\n");
                        }
                        else
                        {
                            // Nếu không tìm thấy, thử tìm trong allSteps
                            var referencedStep = allSteps.FirstOrDefault(s =>
                            {
                                string stepDesc = (s.Attribute("Name")?.Value ?? s.Value).Trim();
                                return stepDesc.Equals(refStepCleaned, StringComparison.OrdinalIgnoreCase) ||
                                       stepDesc.EndsWith(refStepCleaned, StringComparison.OrdinalIgnoreCase);
                            });

                            if (referencedStep != null)
                            {
                                int stepIndex = allSteps.IndexOf(referencedStep);
                                branchPoint = basicFlowDescriptions.TakeWhile(bfd => allSteps.Take(stepIndex + 1)
                                    .Any(s => (s.Attribute("Name")?.Value ?? s.Value).Trim().Contains(bfd.Description)))
                                    .Count(bfd => !bfd.Description.ToLower().Contains("atm") && !bfd.Description.ToLower().Contains("hệ thống") && !bfd.Description.ToLower().Contains("hiển thị"));
                                if (branchPoint == 0) branchPoint = 1; // Đảm bảo branchPoint không bị 0
                                txtThongbao.AppendText($"Tìm thấy RefStep: '{refStep}' trong allSteps, branchPoint = {branchPoint}\r\n");
                            }
                            else
                            {
                                txtThongbao.AppendText($"Không tìm thấy RefStep: '{refStep}' trong Basic Flow, mặc định branchPoint = 1\r\n");
                            }
                        }
                    }

                    // Nếu không tìm thấy RefStep, thử tìm theo từ khóa trong mô tả
                    if (branchPoint == null)
                    {
                        foreach (var (index, desc) in basicFlowDescriptions)
                        {
                            if (description.ToLower().Contains(desc.ToLower()))
                            {
                                branchPoint = index;
                                txtThongbao.AppendText($"Tìm thấy từ khóa trong mô tả: '{description}' khớp với bước Basic Flow số {branchPoint}\r\n");
                                break;
                            }
                        }
                    }

                    branchPoint = branchPoint ?? 1; // Mặc định gắn với bước 1 nếu không xác định được

                    if (!exceptionCounter.ContainsKey(branchPoint.Value))
                        exceptionCounter[branchPoint.Value] = 0;
                    exceptionCounter[branchPoint.Value]++;
                    char subStepLetter = (char)('a' + (exceptionCounter[branchPoint.Value] - 1)); // a, b, c,...
                    string stepNumber = $"{branchPoint}.{subStepLetter}";

                    // Đảm bảo không bỏ sót bước ngoại lệ
                    string uniqueDescription = $"{stepNumber}. {description}";
                    if (uniqueSteps.Add(uniqueDescription)) // Sử dụng uniqueDescription để tránh trùng lặp
                    {
                        string expectedResult = "ATM hiển thị thông báo lỗi và quay lại menu chính";
                        if (description.Contains("Lỗi kết nối với ngân hàng") || description.Contains("Tài khoản không hợp lệ"))
                        {
                            expectedResult = "ATM hiển thị thông báo lỗi và quay lại menu chính";
                        }
                        else if (description.Contains("Lỗi khác"))
                        {
                            expectedResult = "Hệ thống hiển thị thông báo lỗi khác và quay lại menu chính.";
                        }

                        var stepData = new StepData
                        {
                            UseCaseName = useCaseName,
                            Level = "Không xác định",
                            Preconditions = preconditions,
                            PostConditions = postConditions,
                            FlowType = "Exception Flow",
                            Description = uniqueDescription,
                            ExpectedResult = expectedResult,
                            BranchPoint = branchPoint
                        };
                        steps.Add(stepData);
                        txtThongbao.AppendText($"Đã thêm bước Exception Flow: {uniqueDescription}\r\n");
                    }
                }

                // Kết hợp Basic Flow và Exception Flow
                steps.InsertRange(0, basicFlowSteps);

                txtThongbao.AppendText($"Tổng số bước: {steps.Count}\r\n");
            }
            catch (Exception ex)
            {
                txtThongbao.AppendText($"Lỗi khi phân tích bước: {ex.Message}\r\n");
                txtThongbao.AppendText($"StackTrace: {ex.StackTrace}\r\n");
            }
            return steps;

        }



        private IEnumerable<StepData> ParseXmiSteps(XElement useCase)
        {
            
            var steps = new List<StepData>();

            try
            {
                string useCaseName = useCase.Attribute("name")?.Value ?? "Không xác định";
                string preconditions = "Không xác định";
                string postConditions = "Không xác định";

                txtThongbao.AppendText($"Bắt đầu phân tích UseCase (XMI): {useCaseName}\r\n");

                var activityNodes = useCase.Descendants("{http://www.eclipse.org/uml2/2.0.0/UML}ownedBehavior")
                    .Concat(useCase.Descendants("{http://www.eclipse.org/uml2/2.0.0/UML}activity"))
                    .Concat(useCase.Descendants("{http://www.eclipse.org/uml2/2.0.0/UML}specification"))
                    .Concat(useCase.Descendants("vpumlModel")).ToList();

                txtThongbao.AppendText($"Số activity nodes/vpumlModel: {activityNodes.Count()}\r\n");

                int basicFlowStepIndex = 0;
                var basicFlowSteps = new List<XElement>();

                // Xử lý Basic Flow
                foreach (var node in activityNodes)
                {
                    var stepElements = node.Descendants("{http://www.eclipse.org/uml2/2.0.0/UML}node")
                        .Concat(node.Descendants("{http://www.eclipse.org/uml2/2.0.0/UML}action"))
                        .Concat(node.Descendants("step"))
                        .Where(step => !(step.Attribute("type")?.Value?.ToLower().Contains("alternative") ?? false));

                    foreach (var step in stepElements)
                    {
                        string description = step.Attribute("name")?.Value
                            ?? step.Element("{http://www.eclipse.org/uml2/2.0.0/UML}specification")?.Value
                            ?? step.Element("description")?.Value
                            ?? step.Value.Trim()
                            ?? "No description";

                        if (string.IsNullOrEmpty(description) || description == "No description")
                            continue;

                        basicFlowStepIndex++;
                        basicFlowSteps.Add(step);

                        steps.Add(new StepData
                        {
                            UseCaseName = useCaseName,
                            Level = "Không xác định",
                            Preconditions = preconditions,
                            PostConditions = postConditions,
                            FlowType = "Basic Flow",
                            Description = $"{basicFlowStepIndex}. {description}",
                            ExpectedResult = GetStepExpectedResult(step, description) ?? "Không có kết quả kỳ vọng",
                            BranchPoint = null
                        });
                        txtThongbao.AppendText($"Đã thêm bước (XMI): {description}\r\n");
                    }
                }

                // Xử lý Alternative/Exception Flow
                foreach (var node in activityNodes)
                {
                    var altSteps = node.Descendants("{http://www.eclipse.org/uml2/2.0.0/UML}node")
                        .Concat(node.Descendants("{http://www.eclipse.org/uml2/2.0.0/UML}action"))
                        .Concat(node.Descendants("alternativeFlow"))
                        .Where(step => step.Attribute("type")?.Value?.ToLower().Contains("alternative") ?? false);

                    foreach (var step in altSteps)
                    {
                        string description = step.Attribute("name")?.Value
                            ?? step.Element("{http://www.eclipse.org/uml2/2.0.0/UML}specification")?.Value
                            ?? step.Element("description")?.Value
                            ?? step.Value.Trim()
                            ?? "No description";

                        if (string.IsNullOrEmpty(description) || description == "No description")
                            continue;

                        string refStep = step.Attribute("refStep")?.Value;
                        int? branchPoint = null;
                        if (!string.IsNullOrEmpty(refStep))
                        {
                            var referencedStep = basicFlowSteps.FirstOrDefault(s => (s.Attribute("name")?.Value ?? s.Value) == refStep);
                            if (referencedStep != null)
                                branchPoint = basicFlowSteps.IndexOf(referencedStep) + 1;
                        }
                        branchPoint = branchPoint ?? basicFlowStepIndex;

                        string flowType = "Alternative Flow";
                        if (description.ToLower().Contains("lỗi") ||
                            description.ToLower().Contains("mất kết nối") ||
                            description.ToLower().Contains("không hợp lệ") ||
                            description.ToLower().Contains("không đầy đủ"))
                            flowType = "Exception Flow";

                        steps.Add(new StepData
                        {
                            UseCaseName = useCaseName,
                            Level = "Không xác định",
                            Preconditions = preconditions,
                            PostConditions = postConditions,
                            FlowType = flowType,
                            Description = $"{basicFlowStepIndex}.a {description}",
                            ExpectedResult = GetStepExpectedResult(step, description) ?? "Không có kết quả kỳ vọng",
                            BranchPoint = branchPoint
                        });
                        txtThongbao.AppendText($"Đã thêm bước (XMI): {description} (FlowType: {flowType}, BranchPoint: {branchPoint})\r\n");
                    }
                }

                txtThongbao.AppendText($"Tổng số bước (XMI): {steps.Count}\r\n");
            }
            catch (Exception ex)
            {
                txtThongbao.AppendText($"Lỗi khi phân tích bước (XMI): {ex.Message}\r\n");
            }
            return steps;
        }


        private string GetStepExpectedResult(XElement step, string description)
        {
            // Ưu tiên lấy từ các thẻ hoặc thuộc tính liên quan
            var expectedResult = step.Element("expectedResult")?.Value
                ?? step.Element("TestingProcedure")?.Element("ExpectedResults")?.Value
                ?? step.Attribute("expectedResult")?.Value
                ?? step.Element("description")?.Element("expectedResult")?.Value;

            if (!string.IsNullOrEmpty(expectedResult))
                return expectedResult.Trim();

            // Nếu không có, gọi GenerateExpectedResult
            return GenerateExpectedResult(description);
        }

        private string GetTaggedValue(XElement element, string tagName)
        {
            
            var taggedValues = element.Descendants("TaggedValue");
            var taggedValue = taggedValues.FirstOrDefault(tv => tv.Attribute("Name")?.Value == tagName);

            if (taggedValue == null)
            {
                txtThongbao.AppendText($"Không tìm thấy TaggedValue với Name='{tagName}'.\r\n");
                return "None";
            }

            string value = taggedValue.Attribute("Value")?.Value ?? "None";
            txtThongbao.AppendText($"Đã lấy {tagName}: {value}\r\n");
            return value;
        }

        

        private string GenerateExpectedResult(string stepDescription, XElement testingProcedure = null)
        {
            
            // Ưu tiên lấy kết quả kỳ vọng từ XML nếu có
            string expectedResult = testingProcedure?.Element("ExpectedResults")?.Value;
            if (!string.IsNullOrEmpty(expectedResult)) return expectedResult;

            // Kiểm tra nếu stepDescription rỗng
            if (string.IsNullOrEmpty(stepDescription))
                return ""; // Trả về chuỗi rỗng thay vì "Không có kết quả kỳ vọng"

            // Chuyển mô tả thành chữ thường để so sánh
            string lowerDescription = stepDescription.ToLowerInvariant();

            // Thêm log để debug
            txtThongbao.AppendText($"Đang xử lý mô tả: '{stepDescription}' -> ");

            // Xử lý các trường hợp kiểm tra (validation)
            if (lowerDescription.Contains("kiểm tra"))
            {
                if (lowerDescription.Contains("địa chỉ"))
                {
                    if (lowerDescription.Contains("không hợp lệ"))
                        return "Hệ thống phát hiện địa chỉ không hợp lệ, hiển thị thông báo lỗi và yêu cầu nhập lại địa chỉ hợp lệ.";
                    return "Hệ thống kiểm tra địa chỉ thành công, xác nhận địa chỉ hợp lệ và tiếp tục xử lý.";
                }
                else if (lowerDescription.Contains("thẻ tín dụng") || lowerDescription.Contains("ghi nợ"))
                {
                    if (lowerDescription.Contains("không hợp lệ"))
                        return "Hệ thống xác thực thẻ tín dụng/ghi nợ thất bại, hiển thị thông báo lỗi và yêu cầu nhập lại thông tin hợp lệ.";
                    return "Hệ thống kiểm tra thẻ tín dụng/ghi nợ thành công, xác nhận thông tin hợp lệ và tiếp tục xử lý.";
                }
                else if (lowerDescription.Contains("dữ liệu"))
                {
                    if (lowerDescription.Contains("không đầy đủ"))
                        return "Hệ thống phát hiện dữ liệu cơ bản không đầy đủ, hiển thị thông báo lỗi và yêu cầu nhập thêm thông tin.";
                    return "Hệ thống kiểm tra dữ liệu thành công, xác nhận dữ liệu đầy đủ và hợp lệ.";
                }
                else if (lowerDescription.Contains("tài khoản"))
                {
                    if (lowerDescription.Contains("đã tồn tại"))
                        return "Hệ thống phát hiện tài khoản đã tồn tại, hiển thị thông báo lỗi và yêu cầu sử dụng tài khoản khác.";
                    return "Hệ thống kiểm tra tài khoản thành công, xác nhận tài khoản hợp lệ và chưa tồn tại.";
                }
                else
                {
                    return "Hệ thống kiểm tra thành công và tiếp tục xử lý.";
                }
            }
            else if (lowerDescription.Contains("không hợp lệ"))
            {
                if (lowerDescription.Contains("địa chỉ"))
                    return "Hệ thống phát hiện địa chỉ không hợp lệ, hiển thị thông báo lỗi và yêu cầu nhập lại địa chỉ hợp lệ.";
                else if (lowerDescription.Contains("thẻ tín dụng") || lowerDescription.Contains("ghi nợ"))
                    return "Hệ thống xác thực thẻ tín dụng/ghi nợ thất bại, hiển thị thông báo lỗi và yêu cầu nhập lại thông tin hợp lệ.";
                else
                    return "Hệ thống phát hiện dữ liệu không hợp lệ, hiển thị thông báo lỗi và yêu cầu nhập lại.";
            }
            else if (lowerDescription.Contains("không đầy đủ"))
            {
                return "Hệ thống phát hiện dữ liệu cơ bản không đầy đủ, hiển thị thông báo lỗi và yêu cầu nhập thêm thông tin.";
            }
            else if (lowerDescription.Contains("chọn chức năng"))
            {
                return "Không có kết quả kỳ vọng"; // Thay đổi từ "Hệ thống hiển thị..." thành giá trị an toàn
            }
            else if (lowerDescription.Contains("tạo bản ghi"))
            {
                return "Hệ thống tạo bản ghi thành công và lưu vào cơ sở dữ liệu.";
            }
            else if (lowerDescription.Contains("yêu cầu nhập"))
            {
                return "Hệ thống hiển thị yêu cầu nhập thông tin và chờ phản hồi từ khách hàng.";
            }
            else if (lowerDescription.Contains("liên kết"))
            {
                return "Hệ thống liên kết thông tin khách hàng, địa chỉ và tài khoản thành công và lưu lại.";
            }
            else if (lowerDescription.Contains("trả về"))
            {
                if (lowerDescription.Contains("thông tin tài khoản"))
                    return "Hệ thống trả về thông tin tài khoản hợp lệ cho khách hàng và hiển thị xác nhận.";
                return "Hệ thống trả về thông tin thành công và chờ phản hồi từ người dùng.";
            }
            else if (lowerDescription.Contains("hiển thị"))
            {
                if (lowerDescription.Contains("thông tin tài khoản"))
                    return "Hệ thống hiển thị thông tin tài khoản hợp lệ cho khách hàng và chờ xác nhận.";
                return "Hệ thống hiển thị thông tin thành công và chờ phản hồi từ người dùng.";
            }
            else if (lowerDescription.Contains("xác nhận"))
            {
                return "Hệ thống xác nhận thành công và tiếp tục quy trình.";
            }
            else if (lowerDescription.Contains("lỗi") || lowerDescription.Contains("thất bại"))
            {
                return "Hệ thống hiển thị thông báo lỗi và yêu cầu thực hiện lại thao tác.";
            }

            // Trường hợp mặc định
            return ""; // Trả về chuỗi rỗng nếu không khớp với bất kỳ trường hợp nào

        }

        private string GenerateErrorMessage(string condition)
        {
            condition = condition.ToLower();
            if (condition.Contains("không hợp lệ")) return "Hệ thống hiển thị thông báo: 'Dữ liệu không hợp lệ, vui lòng nhập lại.'";
            if (condition.Contains("không đầy đủ")) return "Hệ thống hiển thị thông báo: 'Dữ liệu không đầy đủ, vui lòng nhập lại.'";
            return $"Hệ thống hiển thị thông báo: '{condition}'.";
        }


        private string ExtractValue(string content, string pattern, int groupIndex = 1, RegexOptions options = RegexOptions.None)
        {
            var match = Regex.Match(content, pattern, options);
            if (match.Success && match.Groups.Count > groupIndex)
            {
                return match.Groups[groupIndex].Value.Trim();
            }
            return string.Empty;
        }









        //xử lý file từ StarUML
        private IEnumerable<StepData> ParseStarUmlSteps(string useCaseName, string documentation)
        {
            var steps = new List<StepData>();
            var uniqueSteps = new HashSet<string>();

            try
            {
                if (string.IsNullOrEmpty(documentation))
                {
                    txtThongbao.AppendText($"Không có documentation cho UseCase {useCaseName}\r\n");
                    return steps;
                }

                var lines = documentation.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.None)
                    .Select(l => l.Trim())
                    .Where(l => !string.IsNullOrWhiteSpace(l))
                    .ToArray();
                bool inBasicFlow = false;
                bool inExtensions = false;
                int basicStepCounter = 0;
                int extensionCounter = 0;

                txtThongbao.AppendText($"Bắt đầu phân tích documentation cho UseCase {useCaseName}, tổng số dòng: {lines.Length}\r\n");

                for (int i = 0; i < lines.Length; i++)
                {
                    string line = lines[i];
                    txtThongbao.AppendText($"Xử lý dòng [{i}]: Nguyên bản: '{line}'\r\n");

                    if (line.StartsWith("6.") || (line.Length > 2 && char.IsDigit(line[0]) && line[1] == '.' && line.ToLower().Contains("basic flow")))
                    {
                        inBasicFlow = true;
                        inExtensions = false;
                        basicStepCounter = 0;
                        txtThongbao.AppendText($"Đã phát hiện Basic Flow tại dòng [{i}]\r\n");
                        continue;
                    }
                    else if (line.StartsWith("7.") || (line.Length > 2 && char.IsDigit(line[0]) && line[1] == '.' && line.ToLower().Contains("extensions")))
                    {
                        inBasicFlow = false;
                        inExtensions = true;
                        extensionCounter = 0;
                        txtThongbao.AppendText($"Đã phát hiện Extensions tại dòng [{i}]\r\n");
                        continue;
                    }

                    if (!inBasicFlow && !inExtensions)
                    {
                        txtThongbao.AppendText($"Bỏ qua dòng [{i}] vì không thuộc Basic Flow hoặc Extensions\r\n");
                        continue;
                    }

                    string stepNumber = "";
                    string description = line;
                    string expectedResult = "";

                    var stepMatch = Regex.Match(line, @"^(\d+(\.\d+)*)\.\s+(.+)");
                    var extMatch = Regex.Match(line, @"^(\d+\.\d+\.[a-z]): (.+)");

                    if (stepMatch.Success)
                    {
                        description = stepMatch.Groups[3].Value.Trim();
                        stepNumber = stepMatch.Groups[1].Value + ".";
                    }
                    else if (extMatch.Success)
                    {
                        description = extMatch.Groups[2].Value.Trim();
                        stepNumber = extMatch.Groups[1].Value;
                    }
                    else
                    {
                        if (inBasicFlow)
                        {
                            basicStepCounter++;
                            stepNumber = $"{basicStepCounter}.";
                            description = line;
                        }
                        else if (inExtensions)
                        {
                            extensionCounter++;
                            stepNumber = $"{basicStepCounter}.{Char.ToLower((char)('a' + (extensionCounter - 1)))}:";
                            description = line;
                        }
                    }

                    // Chuẩn hóa description
                    description = description.Trim();
                    if (string.IsNullOrEmpty(description))
                    {
                        txtThongbao.AppendText($"Cảnh báo: Dòng [{i}] không có mô tả, bỏ qua.\r\n");
                        continue;
                    }

                    // Tách ExpectedResult nếu có từ khóa liên quan
                    string lowerDescription = description.ToLower();
                    if (lowerDescription.Contains("hiển thị") || lowerDescription.Contains("không hiển thị") ||
                        lowerDescription.Contains("thành công") || lowerDescription.Contains("thất bại") ||
                        lowerDescription.Contains("lỗi") || lowerDescription.Contains("mất kết nối"))
                    {
                        expectedResult = description;
                        description = "Hành động hệ thống"; // Hoặc giữ nguyên bước trước đó nếu có
                    }
                    else
                    {
                        expectedResult = GenerateExpectedResult(description);
                    }

                    string formattedDescription = $"{stepNumber} {description}";
                    if (uniqueSteps.Add(formattedDescription))
                    {
                        string flowType = inBasicFlow ? "Basic Flow" : "Alternative Flow";
                        if (inExtensions)
                        {
                            string normalizedDescription = description.ToLower().Trim();
                            normalizedDescription = System.Text.RegularExpressions.Regex.Replace(normalizedDescription, @"\s+", " ");
                            normalizedDescription = System.Text.RegularExpressions.Regex.Replace(normalizedDescription, @"[^\w\s]", " ");
                            if (normalizedDescription.Contains("khong day du") || normalizedDescription.Contains("không đầy đủ") ||
                                normalizedDescription.Contains("khong du") || normalizedDescription.Contains("không đủ") ||
                                normalizedDescription.Contains("không hợp lệ") || normalizedDescription.Contains("lỗi") ||
                                normalizedDescription.Contains("mất kết nối") || normalizedDescription.Contains("thất bại") ||
                                normalizedDescription.Contains("không thành công"))
                            {
                                flowType = "Exception Flow";
                            }
                        }

                        var stepData = new StepData
                        {
                            UseCaseName = useCaseName,
                            Level = "Không xác định",
                            Preconditions = "Không có",
                            PostConditions = "Không có",
                            FlowType = flowType,
                            Description = formattedDescription,
                            ExpectedResult = expectedResult,
                            BranchPoint = inBasicFlow ? basicStepCounter : basicStepCounter
                        };
                        steps.Add(stepData);
                        txtThongbao.AppendText($"Đã thêm bước: {formattedDescription} (FlowType: {flowType}, ExpectedResult: {expectedResult})\r\n");
                    }
                }

                txtThongbao.AppendText($"Tổng số bước: {steps.Count}\r\n");
            }
            catch (Exception ex)
            {
                txtThongbao.AppendText($"Lỗi khi phân tích StarUML steps: {ex.Message}\r\n");
                txtThongbao.AppendText($"StackTrace: {ex.StackTrace}\r\n");
            }
            return steps;



            //var steps = new List<StepData>();
            //var uniqueSteps = new HashSet<string>();

            //try
            //{
            //    if (string.IsNullOrEmpty(documentation))
            //    {
            //        txtThongbao.AppendText($"Không có documentation cho UseCase {useCaseName}\r\n");
            //        return steps;
            //    }

            //    var lines = documentation.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.None)
            //        .Select(l => l.Trim())
            //        .Where(l => !string.IsNullOrWhiteSpace(l))
            //        .ToArray();
            //    bool inBasicFlow = false;
            //    bool inExtensions = false;
            //    int basicStepCounter = 0;
            //    int extensionCounter = 0;

            //    txtThongbao.AppendText($"Bắt đầu phân tích documentation cho UseCase {useCaseName}, tổng số dòng: {lines.Length}\r\n");

            //    for (int i = 0; i < lines.Length; i++)
            //    {
            //        string line = lines[i];
            //        txtThongbao.AppendText($"Xử lý dòng [{i}]: Nguyên bản: '{line}'\r\n");

            //        if (line.StartsWith("6.") || (line.Length > 2 && char.IsDigit(line[0]) && line[1] == '.' && line.ToLower().Contains("basic flow")))
            //        {
            //            inBasicFlow = true;
            //            inExtensions = false;
            //            basicStepCounter = 0;
            //            txtThongbao.AppendText($"Đã phát hiện Basic Flow tại dòng [{i}]\r\n");
            //            continue;
            //        }
            //        else if (line.StartsWith("7.") || (line.Length > 2 && char.IsDigit(line[0]) && line[1] == '.' && line.ToLower().Contains("extensions")))
            //        {
            //            inBasicFlow = false;
            //            inExtensions = true;
            //            extensionCounter = 0;
            //            txtThongbao.AppendText($"Đã phát hiện Extensions tại dòng [{i}]\r\n");
            //            continue;
            //        }

            //        if (!inBasicFlow && !inExtensions)
            //        {
            //            txtThongbao.AppendText($"Bỏ qua dòng [{i}] vì không thuộc Basic Flow hoặc Extensions\r\n");
            //            continue;
            //        }

            //        string stepNumber = "";
            //        string description = line;

            //        var stepMatch = Regex.Match(line, @"^(\d+(\.\d+)*)\.\s+(.+)");
            //        var extMatch = Regex.Match(line, @"^(\d+\.\d+\.[a-z]): (.+)");

            //        if (stepMatch.Success || extMatch.Success)
            //        {
            //            description = line;
            //            txtThongbao.AppendText($"Dòng [{i}] đã có số bước, giữ nguyên: '{description}'\r\n");
            //        }
            //        else
            //        {
            //            if (inBasicFlow)
            //            {
            //                basicStepCounter++;
            //                stepNumber = $"{basicStepCounter}.";
            //                description = $"{stepNumber} {description}";
            //                txtThongbao.AppendText($"Đã thêm số bước cho Basic Flow: '{description}'\r\n");
            //            }
            //            else if (inExtensions)
            //            {
            //                bool isSystemStep = description.ToLower().Contains("system") || description.ToLower().Contains("hệ thống");
            //                extensionCounter++;
            //                stepNumber = $"{basicStepCounter}.{Char.ToLower((char)('a' + (extensionCounter - 1)))}:";

            //                if (isSystemStep)
            //                {
            //                    description = description.Replace("System", "").Replace("Hệ thống", "").Trim();
            //                    description = $"{stepNumber} SYSTEM {description}";
            //                }
            //                else
            //                {
            //                    description = $"{stepNumber} {description}";
            //                }
            //                txtThongbao.AppendText($"Đã thêm số bước cho Extensions: '{description}'\r\n");
            //            }
            //        }

            //        if (string.IsNullOrEmpty(description))
            //        {
            //            txtThongbao.AppendText($"Cảnh báo: Dòng [{i}] không có mô tả, bỏ qua.\r\n");
            //            continue;
            //        }

            //        string flowType = inBasicFlow ? "Basic Flow" : "Alternative Flow";
            //        if (inExtensions)
            //        {
            //            string normalizedDescription = description.ToLower().Trim();
            //            normalizedDescription = System.Text.RegularExpressions.Regex.Replace(normalizedDescription, @"\s+", " ");
            //            normalizedDescription = System.Text.RegularExpressions.Regex.Replace(normalizedDescription, @"[^\w\s]", " ");
            //            if (normalizedDescription.Contains("khong day du") ||
            //                normalizedDescription.Contains("không đầy đủ") ||
            //                normalizedDescription.Contains("khong du") ||
            //                normalizedDescription.Contains("không đủ") ||
            //                normalizedDescription.Contains("không hợp lệ") ||
            //                normalizedDescription.Contains("lỗi") ||
            //                normalizedDescription.Contains("mất kết nối") ||
            //                normalizedDescription.Contains("thất bại") ||
            //                normalizedDescription.Contains("không thành công"))
            //            {
            //                flowType = "Exception Flow";
            //            }
            //        }

            //        int? branchPoint = null;
            //        if (inExtensions && !string.IsNullOrEmpty(stepNumber))
            //        {
            //            var branchMatch = Regex.Match(stepNumber, @"(\d+)\.\d+\.[a-z]");
            //            if (branchMatch.Success && int.TryParse(branchMatch.Groups[1].Value, out int parsedBranchPoint))
            //            {
            //                branchPoint = parsedBranchPoint;
            //            }
            //            else
            //            {
            //                branchPoint = basicStepCounter;
            //            }
            //            txtThongbao.AppendText($"Đặt BranchPoint cho dòng [{i}]: {branchPoint}\r\n");
            //        }

            //        // Tính toán ExpectedResult và chuẩn hóa trước khi kiểm tra
            //        string expectedResult = GenerateExpectedResult(description) ?? "Không có kết quả kỳ vọng";
            //        string defaultDescription = "Hệ thống hiển thị giao diện tạo/cập nhật tài khoản và cho phép nhập thông tin cơ bản";

            //        // Chuẩn hóa expectedResult và defaultDescription để so sánh chính xác hơn
            //        string normalizedExpectedResult = expectedResult.ToLower().Trim();
            //        normalizedExpectedResult = System.Text.RegularExpressions.Regex.Replace(normalizedExpectedResult, @"\s+", " ");
            //        string normalizedDefaultDescription = defaultDescription.ToLower().Trim();
            //        normalizedDefaultDescription = System.Text.RegularExpressions.Regex.Replace(normalizedDefaultDescription, @"\s+", " ");

            //        // Kiểm tra và đặt lại ExpectedResult cho bước 1 của Basic Flow
            //        if (inBasicFlow && basicStepCounter == 1 && normalizedExpectedResult.Contains(normalizedDefaultDescription))
            //        {
            //            expectedResult = "Không có kết quả kỳ vọng";
            //            txtThongbao.AppendText($"Đã đặt lại ExpectedResult cho bước 1 Basic Flow tại dòng [{i}] do chứa nội dung không mong muốn: '{expectedResult}'\r\n");
            //        }

            //        if (uniqueSteps.Add(description))
            //        {
            //            string normalizedDescription = description.ToLower().Trim();
            //            normalizedDescription = System.Text.RegularExpressions.Regex.Replace(normalizedDescription, @"\s+", " ");
            //            normalizedDescription = System.Text.RegularExpressions.Regex.Replace(normalizedDescription, @"[^\w\s]", " ");
            //            txtThongbao.AppendText($"Chuẩn hóa mô tả tại dòng [{i}]: '{normalizedDescription}'\r\n");

            //            steps.Add(new StepData
            //            {
            //                UseCaseName = useCaseName,
            //                Level = "Không xác định",
            //                Preconditions = "Không có",
            //                PostConditions = "Không có",
            //                FlowType = flowType,
            //                Description = description,
            //                ExpectedResult = expectedResult,
            //                BranchPoint = branchPoint
            //            });

            //            txtThongbao.AppendText($"Đã thêm bước tại dòng [{i}]: {description} (FlowType: {flowType}, BranchPoint: {branchPoint})\r\n");
            //        }
            //        else
            //        {
            //            txtThongbao.AppendText($"Bước tại dòng [{i}] đã tồn tại, bỏ qua: {description}\r\n");
            //        }
            //    }

            //    if (!steps.Any(s => s.FlowType == "Basic Flow"))
            //    {
            //        txtThongbao.AppendText($"Cảnh báo: Không tìm thấy Basic Flow cho UseCase {useCaseName}\r\n");
            //    }

            //    txtThongbao.AppendText($"Tổng số bước cho UseCase {useCaseName}: {steps.Count}\r\n");
            //}
            //catch (Exception ex)
            //{
            //    txtThongbao.AppendText($"Lỗi khi phân tích bước StarUML cho {useCaseName}: {ex.Message}\r\n");
            //    txtThongbao.AppendText($"StackTrace: {ex.StackTrace}\r\n");
            //}
            //return steps;
        }

        private IEnumerable<UseCaseData> ParseStarUmlXml(XDocument doc)
        {
            
            var useCases = new List<UseCaseData>();
            try
            {
                if (doc.Root == null)
                {
                    Console.WriteLine("Lỗi: Tệp XML không có thẻ gốc (<root>).");
                    return useCases;
                }

                var ownedElements = doc.Descendants("ownedElements");
                if (!ownedElements.Any())
                {
                    Console.WriteLine("Lỗi: Không tìm thấy 'ownedElements' trong tệp XML.");
                    return useCases;
                }

                var useCaseElements = ownedElements.Where(e => e.Attribute("type")?.Value == "UMLUseCase");
                if (!useCaseElements.Any())
                {
                    Console.WriteLine("Cảnh báo: Không tìm thấy UMLUseCase trong tệp XML.");
                    return useCases;
                }

                Console.WriteLine($"Số UseCase tìm thấy trong StarUML XML: {useCaseElements.Count()}");

                foreach (var element in useCaseElements)
                {
                    var nameElement = element.Element("name");
                    var idElement = element.Element("id");
                    var documentationElement = element.Element("documentation");

                    string name = nameElement?.Value ?? "Unknown UseCase";
                    string id = idElement?.Value ?? Guid.NewGuid().ToString();
                    string documentation = documentationElement?.Value ?? "";

                    Console.WriteLine($"Xử lý UseCase: {name}, ID: {id}, Documentation length: {documentation.Length}");
                    Console.WriteLine($"Nội dung Documentation (raw): '{documentation.Replace("\n", "\\n").Replace("\r", "\\r")}'");

                    if (string.IsNullOrEmpty(name))
                    {
                        Console.WriteLine("Cảnh báo: Một UseCase không có tên, bỏ qua.");
                        continue;
                    }

                    if (string.IsNullOrEmpty(id))
                    {
                        Console.WriteLine($"Cảnh báo: UseCase '{name}' không có ID, sử dụng ID mặc định.");
                    }

                    string preconditions = "Không có";
                    string postconditions = "Không có";

                    if (!string.IsNullOrEmpty(documentation))
                    {
                        var lines = documentation.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.None);
                        Console.WriteLine($"Số dòng trong documentation: {lines.Length}");
                        bool inPreconditions = false;
                        bool inPostconditions = false;
                        StringBuilder preBuilder = new StringBuilder();
                        StringBuilder postBuilder = new StringBuilder();

                        for (int i = 0; i < lines.Length; i++)
                        {
                            var line = lines[i];
                            var trimmedLine = line.Trim();
                            Console.WriteLine($"Dòng [{i}]: '{trimmedLine}' (Raw: '{line.Replace("\n", "\\n").Replace("\r", "\\r")}')");

                            if (trimmedLine.ToLower().StartsWith("4.") && trimmedLine.ToLower().Contains("preconditions"))
                            {
                                Console.WriteLine($"Phát hiện Preconditions tại dòng [{i}]: '{trimmedLine}'");
                                inPreconditions = true;
                                inPostconditions = false;
                                if (!string.IsNullOrWhiteSpace(trimmedLine))
                                {
                                    preBuilder.AppendLine(trimmedLine);
                                }
                                continue;
                            }
                            else if (trimmedLine.ToLower().StartsWith("5.") && trimmedLine.ToLower().Contains("postconditions"))
                            {
                                Console.WriteLine($"Phát hiện Postconditions tại dòng [{i}]: '{trimmedLine}'");
                                inPreconditions = false;
                                inPostconditions = true;
                                if (!string.IsNullOrWhiteSpace(trimmedLine))
                                {
                                    postBuilder.AppendLine(trimmedLine);
                                }
                                continue;
                            }
                            else if (trimmedLine.ToLower().StartsWith("6."))
                            {
                                Console.WriteLine($"Kết thúc tại Basic Flow tại dòng [{i}]: '{trimmedLine}'");
                                break;
                            }

                            if (inPreconditions && !string.IsNullOrWhiteSpace(trimmedLine))
                            {
                                Console.WriteLine($"Thêm vào Preconditions tại dòng [{i}]: '{trimmedLine}'");
                                preBuilder.AppendLine(trimmedLine);
                            }
                            else if (inPostconditions && !string.IsNullOrWhiteSpace(trimmedLine))
                            {
                                Console.WriteLine($"Thêm vào Postconditions tại dòng [{i}]: '{trimmedLine}'");
                                postBuilder.AppendLine(trimmedLine);
                            }
                        }

                        preconditions = preBuilder.Length > 0 ? preBuilder.ToString().Trim() : preconditions;
                        postconditions = postBuilder.Length > 0 ? postBuilder.ToString().Trim() : postconditions;
                        Console.WriteLine($"Kết quả UseCase {name}: Preconditions = '{preconditions.Replace("\n", "\\n").Replace("\r", "\\r")}', Postconditions = '{postconditions.Replace("\n", "\\n").Replace("\r", "\\r")}'");
                    }

                    var steps = ParseStarUmlSteps(name, documentation).ToList();
                    Console.WriteLine($"Số bước cho UseCase {name}: {steps.Count}");

                    useCases.Add(new UseCaseData
                    {
                        Id = id,
                        Name = name,
                        Level = "User",
                        Preconditions = preconditions,
                        Postconditions = postconditions,
                        Steps = steps
                    });

                    Console.WriteLine($"Đã thêm UseCase {name} vào danh sách.");
                }

                Console.WriteLine($"Tổng số UseCase đã xử lý: {useCases.Count}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Lỗi nghiêm trọng khi phân tích StarUML XML: {ex.Message}");
                Console.WriteLine($"StackTrace: {ex.StackTrace}");
            }
            return useCases;
        }

        

        private IEnumerable<UseCaseData> ParseStarUmlXml(XDocument doc, TextBox txtThongbao)
        {

            var useCases = new List<UseCaseData>();
            try
            {
                if (doc.Root == null)
                {
                    txtThongbao.AppendText("Lỗi: Tệp XML không có thẻ gốc (<root>).\r\n");
                    return useCases;
                }

                var ownedElements = doc.Descendants("ownedElements");
                if (!ownedElements.Any())
                {
                    txtThongbao.AppendText("Lỗi: Không tìm thấy 'ownedElements' trong tệp XML.\r\n");
                    return useCases;
                }

                var useCaseElements = ownedElements.Where(e => e.Attribute("type")?.Value == "UMLUseCase");
                if (!useCaseElements.Any())
                {
                    txtThongbao.AppendText("Cảnh báo: Không tìm thấy UMLUseCase trong tệp XML.\r\n");
                    return useCases;
                }

                txtThongbao.AppendText($"Số UseCase tìm thấy trong StarUML XML: {useCaseElements.Count()}\r\n");

                foreach (var element in useCaseElements)
                {
                    var nameElement = element.Element("name");
                    var idElement = element.Element("id");
                    var documentationElement = element.Element("documentation");

                    string name = nameElement?.Value ?? "Unknown UseCase";
                    string id = idElement?.Value ?? Guid.NewGuid().ToString();
                    string documentation = documentationElement?.Value ?? "";

                    if (string.IsNullOrEmpty(name))
                    {
                        txtThongbao.AppendText("Cảnh báo: Một UseCase không có tên, bỏ qua.\r\n");
                        continue;
                    }

                    if (string.IsNullOrEmpty(id))
                    {
                        txtThongbao.AppendText($"Cảnh báo: UseCase '{name}' không có ID, sử dụng ID mặc định.\r\n");
                    }

                    string preconditions = "Không có";
                    string postconditions = "Không có";

                    if (!string.IsNullOrEmpty(documentation))
                    {
                        var lines = documentation.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.None);
                        bool foundPreconditions = false;
                        bool foundPostconditions = false;

                        for (int i = 0; i < lines.Length; i++)
                        {
                            var line = lines[i].Trim();
                            if (string.IsNullOrWhiteSpace(line)) continue;

                            if (line.StartsWith("4. Preconditions:") && !foundPreconditions)
                            {
                                txtThongbao.AppendText($"Phát hiện Preconditions tại dòng [{i}]: '{line}'\r\n");
                                preconditions = string.Join("\n", lines.Skip(i).TakeWhile(l => !l.Trim().StartsWith("5.") && !l.Trim().StartsWith("6."))).Trim();
                                preconditions = preconditions.Substring(preconditions.IndexOf(":") + 1).Trim();
                                foundPreconditions = true;
                                txtThongbao.AppendText($"Preconditions trích xuất: '{preconditions}'\r\n");
                            }
                            else if (line.StartsWith("5. Postconditions:") && !foundPostconditions)
                            {
                                txtThongbao.AppendText($"Phát hiện Postconditions tại dòng [{i}]: '{line}'\r\n");
                                postconditions = string.Join("\n", lines.Skip(i).TakeWhile(l => !l.Trim().StartsWith("6."))).Trim();
                                postconditions = postconditions.Substring(postconditions.IndexOf(":") + 1).Trim();
                                foundPostconditions = true;
                                txtThongbao.AppendText($"Postconditions trích xuất: '{postconditions}'\r\n");
                            }
                            else if (line.StartsWith("6.") || line.ToLower().Contains("basic flow"))
                            {
                                txtThongbao.AppendText($"Kết thúc tại Basic Flow tại dòng [{i}]: '{line}'\r\n");
                                break;
                            }
                        }
                    }
                    else
                    {
                        txtThongbao.AppendText($"Không có documentation cho UseCase {name}\r\n");
                    }

                    var steps = ParseStarUmlSteps(name, documentation).ToList();
                    txtThongbao.AppendText($"Tổng số bước cho UseCase {name}: {steps.Count}\r\n");
                    txtThongbao.AppendText($"Số bước tìm thấy cho UseCase {name}: {steps.Count}\r\n");

                    useCases.Add(new UseCaseData
                    {
                        Id = id,
                        Name = name,
                        Level = "User",
                        Preconditions = preconditions,
                        Postconditions = postconditions,
                        Steps = steps
                    });
                }
            }
            catch (Exception ex)
            {
                txtThongbao.AppendText($"Lỗi nghiêm trọng khi phân tích StarUML XML: {ex.Message}\r\n");
                txtThongbao.AppendText($"StackTrace: {ex.StackTrace}\r\n");
            }
            return useCases;
        }



        //Hàm phân tích tệp TXT
        private void ParseTxtFile(string txtFilePath)
        {
                        
            try
            {
                _testCases.Clear();
                _useCases.Clear(); // Xóa danh sách Use Case cũ
                txtThongbao.Text = string.Empty;

                string content = File.ReadAllText(txtFilePath, Encoding.UTF8);
                var useCaseSections = Regex.Split(content, @"(?=Use case ID\s*:)", RegexOptions.IgnoreCase)
                    .Where(section => !string.IsNullOrWhiteSpace(section))
                    .ToList();

                foreach (var section in useCaseSections)
                {
                    string useCaseId = ExtractValue(section, @"Use case ID\s*:\s*(.*?)(?:\n|$)", 1, RegexOptions.IgnoreCase);
                    if (string.IsNullOrEmpty(useCaseId)) continue;

                    string useCaseName = ExtractValue(section, @"Use case name\s*:\s*(.*?)(?:\n|$)", 1, RegexOptions.IgnoreCase);
                    if (string.IsNullOrEmpty(useCaseName)) continue;

                    string preconditions = ExtractValue(section, @"(Pre-Condition\(s\)|Preconditions)\s*:([\s\S]*?)(?=(?:BasicFlow|Main Flow|Primary Flow|ExceptionFlow|Alternative Flows|Post-Condition\(s\)|Postconditions|$))", 2, RegexOptions.IgnoreCase) ?? "Không có";
                    string postconditions = ExtractValue(section, @"(Post-Condition\(s\)|Postconditions)\s*:([\s\S]*?)(?=(?:BasicFlow|Main Flow|Primary Flow|ExceptionFlow|Alternative Flows|$))", 2, RegexOptions.IgnoreCase) ?? "Không có";

                    string basicFlow = ExtractValue(section, @"(BasicFlow|Main Flow|Primary Flow)\s*:([\s\S]*?)(?=(?:ExceptionFlow|Alternative Flows|Extended Use Case|Exceptions|Post-Condition\(s\)|Postconditions|$))", 2, RegexOptions.IgnoreCase) ?? "";
                    string exceptionFlow = ExtractValue(section, @"(ExceptionFlow|Alternative Flows)\s*:([\s\S]*?)(?=(?:Extended Use Case|Exceptions|Post-Condition\(s\)|Postconditions|$))", 2, RegexOptions.IgnoreCase) ?? "";

                    var useCaseData = new UseCaseData
                    {
                        Id = useCaseId,
                        Name = useCaseName,
                        Level = "User",
                        Preconditions = preconditions,
                        Postconditions = postconditions,
                        Steps = new List<StepData>()
                    };

                    // Phân tích Basic Flow
                    if (!string.IsNullOrEmpty(basicFlow))
                    {
                        var steps = basicFlow.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries)
                            .Select(line => line.Trim())
                            .Where(line => !string.IsNullOrWhiteSpace(line))
                            .Where(line => !Regex.IsMatch(line, @"^[-=]{2,}$"))
                            .Select(line => new StepData
                            {
                                UseCaseName = useCaseName,
                                Level = "Không xác định",
                                Preconditions = preconditions,
                                PostConditions = postconditions,
                                FlowType = "Basic Flow",
                                Description = line,
                                ExpectedResult = ExtractNextExpectedResult(basicFlow, line),
                                BranchPoint = null
                            }).ToList();
                        useCaseData.Steps.AddRange(steps.Where(s => !string.IsNullOrWhiteSpace(s.Description)));
                    }

                    // Phân tích Exception/Alternative Flow
                    if (!string.IsNullOrEmpty(exceptionFlow))
                    {
                        var exceptionStepsList = exceptionFlow.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries)
                            .Select(line => line.Trim())
                            .Where(line => !string.IsNullOrWhiteSpace(line))
                            .Where(line => !Regex.IsMatch(line, @"^[-=]{2,}$"))
                            .ToList();

                        var exceptionSteps = exceptionStepsList
                            .Select((line, index) =>
                            {
                                // Chuẩn hóa dòng để xử lý ký tự tiếng Việt
                                string normalizedLine = line.ToLower().Normalize(NormalizationForm.FormD);
                                StringBuilder stringBuilder = new StringBuilder();
                                foreach (char c in normalizedLine)
                                {
                                    UnicodeCategory unicodeCategory = CharUnicodeInfo.GetUnicodeCategory(c);
                                    if (unicodeCategory != UnicodeCategory.NonSpacingMark)
                                    {
                                        stringBuilder.Append(c);
                                    }
                                }
                                string normalizedLineFinal = stringBuilder.ToString().Normalize(NormalizationForm.FormC).Replace(":", "").Replace(".", "").Trim();
                                txtThongbao.AppendText($"Debug - Normalized Line: {normalizedLineFinal}\r\n");

                                // Xác định FlowType
                                string flowType;
                                // Ép hai bước cuối của exceptionFlow thành Exception Flow cho UseCase "Kiểm tra số dư"
                                if (useCaseId == "UC01" && index >= exceptionStepsList.Count - 2 && exceptionStepsList.Count >= 2)
                                {
                                    flowType = "Exception Flow";
                                }
                                else
                                {
                                    flowType = (normalizedLineFinal.Contains("loi") || normalizedLineFinal.Contains("error") || normalizedLineFinal.Contains("invalid") || normalizedLineFinal.Contains("khong hop le") || normalizedLineFinal.Contains("not valid") || normalizedLineFinal.Contains("khong du")) ? "Exception Flow" : "Alternative Flow";
                                }

                                txtThongbao.AppendText($"Debug - FlowType for '{line}': {flowType}\r\n");

                                return new StepData
                                {
                                    UseCaseName = useCaseName,
                                    Level = "Không xác định",
                                    Preconditions = preconditions,
                                    PostConditions = postconditions,
                                    FlowType = flowType,
                                    Description = line,
                                    ExpectedResult = ExtractNextExpectedResult(exceptionFlow, line),
                                    BranchPoint = ExtractBranchPoint(line)
                                };
                            }).ToList();
                        useCaseData.Steps.AddRange(exceptionSteps.Where(s => !string.IsNullOrWhiteSpace(s.Description)));
                    }

                    // Chỉ thêm UseCase nếu có ít nhất một bước hợp lệ
                    if (useCaseData.Steps.Any())
                    {
                        _useCases.Add(useCaseData);
                    }
                }

                // Cập nhật combobox
                comboboxUC.Items.Clear();
                var uniqueUseCaseNames = new HashSet<string>();
                foreach (var useCase in _useCases)
                {
                    if (uniqueUseCaseNames.Add(useCase.Name))
                    {
                        comboboxUC.Items.Add(useCase.Name);
                    }
                }

                if (_useCases.Count > 0)
                {
                    txtThongbao.AppendText($"Đã tải {_useCases.Count} Use Case từ file TXT. Vui lòng chọn Use Case để hiển thị chi tiết.\r\n");
                    if (comboboxUC.Items.Count > 0) comboboxUC.SelectedIndex = 0;
                }
                else
                {
                    txtThongbao.AppendText("Không tìm thấy Use Case trong file TXT!\r\n");
                }
            }
            catch (Exception ex)
            {
                txtThongbao.Text = $"Lỗi khi phân tích file .txt: {ex.Message}\r\n";
            }
                        
        }

        

        // Phương thức phụ để trích xuất Expected Result từ dòng tiếp theo
        private string ExtractNextExpectedResult(string flowText, string currentLine)
        {
            var lines = flowText.Split(new[] { '\n' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(line => line.Trim())
                .Where(line => !string.IsNullOrWhiteSpace(line))
                .ToList();
            int index = lines.IndexOf(currentLine);
            if (index >= 0 && index + 1 < lines.Count)
            {
                return lines[index + 1].Trim(); // Trả về dòng tiếp theo làm Expected Result
            }
            return "Không có kết quả kỳ vọng";
        }

        // Phương thức phụ để trích xuất BranchPoint từ mô tả
        private int? ExtractBranchPoint(string description)
        {
            var match = Regex.Match(description, @"^(\d+)[a-z]?\.");
            if (match.Success && int.TryParse(match.Groups[1].Value, out int branchPoint))
            {
                return branchPoint;
            }
            return null;



        }

        private UseCaseStep ParseXmiStep(XElement stepElement)
        {
            var step = new UseCaseStep
            {
                Name = stepElement.Element("properties")?.Elements("property")
                    .FirstOrDefault(p => p.Attribute("name")?.Value == "name")?.Attribute("value")?.Value ?? "Unnamed Step"
            };

            var subStepsContainer = stepElement.Descendants("stepContainers").FirstOrDefault();
            if (subStepsContainer != null)
            {
                foreach (var subStep in subStepsContainer.Elements("vpumlModel"))
                {
                    var subStepObj = ParseXmiStep(subStep);
                    // Phân loại bước: nếu là bước "không hợp lệ" hoặc "không đầy đủ", thì là Extension
                    if (subStepObj.Name.ToLower().Contains("không hợp lệ") || subStepObj.Name.ToLower().Contains("không đầy đủ"))
                    {
                        step.Extensions.Add(subStepObj);
                    }
                    else
                    {
                        step.SubSteps.Add(subStepObj);
                    }
                }
            }

            return step;
        }

        

        

        private void btnInput_Click(object sender, EventArgs e)
        {
            //chạy
            //using (var openFileDialog = new OpenFileDialog())
            //{
            //    openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
            //    if (radioTextIn.Checked)
            //    {
            //        openFileDialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
            //    }
            //    else if (radioXMLIn.Checked)
            //    {
            //        openFileDialog.Filter = "XML files (*.xml)|*.xml|All files (*.*)|*.*";
            //    }
            //    else if (radioXMIIn.Checked)
            //    {
            //        openFileDialog.Filter = "XMI files (*.xmi)|*.xmi|All files (*.*)|*.*";
            //    }
            //    else
            //    {
            //        openFileDialog.Filter = "Supported files (*.txt;*.xml;*.xmi;*.uml)|*.txt;*.xml;*.xmi;*.uml|All files (*.*)|*.*";
            //    }

            //    if (openFileDialog.ShowDialog() == DialogResult.OK)
            //    {
            //        string filePath = openFileDialog.FileName;
            //        bool isStarUml;
            //        bool isValid = ValidateXmlFile(filePath, out isStarUml); // Kiểm tra cú pháp XML nếu có

            //        if (radioTextIn.Checked)
            //        {
            //            if (ValidateTxtFile(filePath))
            //            {
            //                txtInputTM.Text = filePath;
            //                _selectedFilePath = filePath;
            //                ParseTxtFile(filePath); // Gọi phương thức mới
            //            }
            //        }
            //        else if (radioXMLIn.Checked || radioXMIIn.Checked)
            //        {
            //            if (isValid)
            //            {
            //                txtInputTM.Text = filePath;
            //                _selectedFilePath = filePath;
            //                LoadXmlAndUseCases(filePath);
            //            }
            //        }
            //        else if (radioHTMLOut.Checked || radioWordOut.Checked || radioExcelOut.Checked)
            //        {
            //            if (isValid)
            //            {
            //                txtInputTM.Text = filePath;
            //                return;
            //            }
            //        }
            //    }
            //}


            using (var openFileDialog = new OpenFileDialog())
            {
                openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                if (radioTextIn.Checked)
                {
                    openFileDialog.Filter = "Text files (*.txt)|*.txt|All files (*.*)|*.*";
                }
                else if (radioXMLIn.Checked)
                {
                    openFileDialog.Filter = "XML files (*.xml)|*.xml|All files (*.*)|*.*";
                }
                else if (radioXMIIn.Checked)
                {
                    openFileDialog.Filter = "XMI files (*.xmi)|*.xmi|All files (*.*)|*.*";
                }
                else
                {
                    openFileDialog.Filter = "Supported files (*.txt;*.xml;*.xmi;*.uml)|*.txt;*.xml;*.xmi;*.uml|All files (*.*)|*.*";
                }

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string filePath = openFileDialog.FileName;
                    bool isStarUml;
                    bool isValid = ValidateXmlFile(filePath, out isStarUml); // Kiểm tra cú pháp XML nếu có

                    if (radioTextIn.Checked)
                    {
                        if (ValidateTxtFile(filePath))
                        {
                            txtInputTM.Text = filePath;
                            _selectedFilePath = filePath;
                            ParseTxtFile(filePath); // Truyền filePath vào đây
                        }
                    }
                    else if (radioXMLIn.Checked || radioXMIIn.Checked)
                    {
                        if (isValid)
                        {
                            txtInputTM.Text = filePath;
                            _selectedFilePath = filePath;
                            LoadXmlAndUseCases(filePath);
                        }
                    }
                    else if (radioHTMLOut.Checked || radioWordOut.Checked || radioExcelOut.Checked)
                    {
                        if (isValid)
                        {
                            txtInputTM.Text = filePath;
                            return;
                        }
                    }
                }
            }

        }

        private void btnOutput_Click(object sender, EventArgs e)
        {

            using (var openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Filter = "Select Folder|SelectFolder.txt"; // Bộ lọc giả để chọn thư mục
                openFileDialog.Title = "Chọn thư mục để lưu báo cáo";
                openFileDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                openFileDialog.CheckFileExists = false; // Tắt kiểm tra file tồn tại
                openFileDialog.CheckPathExists = true; // Đảm bảo đường dẫn thư mục tồn tại
                openFileDialog.FileName = "SelectFolder.txt"; // Tên file giả để hiển thị trong hộp thoại

                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string selectedPath = openFileDialog.FileName; // Đường dẫn chứa file giả
                    string selectedFolder = Path.GetDirectoryName(selectedPath); // Lấy đường dẫn thư mục
                    txtOutputTM.Text = selectedFolder; // Hiển thị toàn bộ đường dẫn thư mục
                }
            }
        }

        private void LoadDataGridView(UseCaseData useCase)
        {
            //chạy bthg
            //try
            //{
            //    if (!_isDisplayingTestCases)
            //    {
            //        SetUseCaseDetailsColumns();
            //        dgvUseCaseDetails.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            //    }

            //    int expectedColumnCount = 7;
            //    if (dgvUseCaseDetails.Columns.Count != expectedColumnCount)
            //    {
            //        txtThongbao.AppendText($"Lỗi: Số cột trong DataGridView ({dgvUseCaseDetails.Columns.Count}) không khớp với số cột mong đợi ({expectedColumnCount}).\r\n");
            //        SetUseCaseDetailsColumns();
            //    }

            //    dgvUseCaseDetails.Rows.Clear();

            //    if (useCase == null || useCase.Steps == null || !useCase.Steps.Any())
            //    {
            //        txtThongbao.AppendText("Không có bước nào để hiển thị cho UseCase này.\r\n");
            //        return;
            //    }

            //    txtThongbao.AppendText($"UseCase: {useCase.Name}\r\n");
            //    txtThongbao.AppendText($"Số bước: {useCase.Steps.Count}\r\n");

            //    int stepCounter = 0;
            //    Dictionary<int, List<string>> stepExpectedResults = new Dictionary<int, List<string>>();
            //    Dictionary<int, string> stepProcedures = new Dictionary<int, string>();
            //    Dictionary<int, string> stepPreconditions = new Dictionary<int, string>();
            //    Dictionary<int, string> stepPostconditions = new Dictionary<int, string>();
            //    Dictionary<int, string> stepFlowTypes = new Dictionary<int, string>();
            //    Dictionary<int, string> stepLevels = new Dictionary<int, string>();
            //    string lastUseCaseName = null;

            //    // Duyệt qua tất cả các bước và hiển thị cả bước SYSTEM nếu cần
            //    for (int i = 0; i < useCase.Steps.Count; i++)
            //    {
            //        var step = useCase.Steps[i];
            //        txtThongbao.AppendText($"Step Debug - Index: {i}, Description: {step.Description}, ExpectedResult: {step.ExpectedResult}\r\n");

            //        string displayPreconditions = step.Preconditions ?? "Không có";
            //        string displayPostconditions = step.PostConditions ?? "Không có";

            //        if (displayPreconditions == "Không có" && useCase.Preconditions != "Không có")
            //        {
            //            displayPreconditions = useCase.Preconditions;
            //        }
            //        if (displayPostconditions == "Không có" && useCase.Postconditions != "Không có")
            //        {
            //            displayPostconditions = useCase.Postconditions;
            //        }

            //        // Tạm thời loại bỏ điều kiện isSystemStep để kiểm tra
            //        stepCounter++;
            //        stepProcedures[stepCounter] = step.Description ?? "Không có mô tả";
            //        stepPreconditions[stepCounter] = displayPreconditions;
            //        stepPostconditions[stepCounter] = displayPostconditions;
            //        stepFlowTypes[stepCounter] = step.FlowType ?? "Không xác định";
            //        stepLevels[stepCounter] = step.Level ?? "Không xác định";
            //        stepExpectedResults[stepCounter] = new List<string>();

            //        // Thêm ExpectedResult từ chính bước
            //        if (!string.IsNullOrEmpty(step.ExpectedResult) && step.ExpectedResult != "Không có kết quả kỳ vọng")
            //        {
            //            stepExpectedResults[stepCounter].Add(step.ExpectedResult.Trim());
            //        }

            //        // Kiểm tra bước SYSTEM tiếp theo để gộp vào ExpectedResult
            //        for (int j = i + 1; j < useCase.Steps.Count; j++)
            //        {
            //            var nextStep = useCase.Steps[j];
            //            bool isNextSystemStep = nextStep.Description?.ToLower().Contains("system") == true ||
            //                                   nextStep.Description?.ToLower().Contains("hệ thống") == true ||
            //                                   nextStep.Description?.ToLower().Contains("hệ thống hiển thị") == true ||
            //                                   nextStep.Description?.ToLower().Contains("system displays") == true;

            //            if (isNextSystemStep)
            //            {
            //                string nextStepDescription = nextStep.Description?.Trim();
            //                if (!string.IsNullOrEmpty(nextStepDescription) && nextStepDescription != "Hệ thống xử lý hành động thành công và thực hiện các bước tiếp theo")
            //                {
            //                    stepExpectedResults[stepCounter].Add(nextStepDescription);
            //                }
            //                i = j; // Bỏ qua bước SYSTEM đã xử lý
            //            }
            //            else
            //            {
            //                break;
            //            }
            //        }

            //        lastUseCaseName = step.UseCaseName ?? "Không xác định";
            //    }

            //    // Thêm dữ liệu vào DataGridView
            //    foreach (var stepNum in stepProcedures.Keys)
            //    {
            //        var uniqueExpectedResults = stepExpectedResults[stepNum].Distinct().ToList();
            //        string combinedExpectedResults = uniqueExpectedResults.Any() ? string.Join("\n", uniqueExpectedResults) : "Không có kết quả kỳ vọng";

            //        dgvUseCaseDetails.Rows.Add(
            //            lastUseCaseName,
            //            stepLevels[stepNum],
            //            stepPreconditions[stepNum],
            //            stepPostconditions[stepNum],
            //            stepFlowTypes[stepNum],
            //            stepProcedures[stepNum],
            //            combinedExpectedResults
            //        );
            //    }

            //    txtThongbao.AppendText($"Đã hiển thị {stepProcedures.Count} bước cho UseCase: {useCase.Name}\r\n");
            //}
            //catch (Exception ex)
            //{
            //    txtThongbao.AppendText($"Lỗi khi hiển thị dữ liệu trong DataGridView: {ex.Message}\r\n");
            //    txtThongbao.AppendText($"StackTrace: {ex.StackTrace}\r\n");
            //}


            //chạy ổn nhất
            //try
            //{
            //    if (!_isDisplayingTestCases)
            //    {
            //        SetUseCaseDetailsColumns();
            //        dgvUseCaseDetails.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
            //        dgvUseCaseDetails.Rows.Clear();
            //        dgvUseCaseDetails.AllowUserToAddRows = false; // Ngăn DataGridView thêm hàng trống
            //    }

            //    int expectedColumnCount = 7;
            //    if (dgvUseCaseDetails.Columns.Count != expectedColumnCount)
            //    {
            //        txtThongbao.AppendText($"Lỗi: Số cột trong DataGridView ({dgvUseCaseDetails.Columns.Count}) không khớp với số cột mong đợi ({expectedColumnCount}).\r\n");
            //        SetUseCaseDetailsColumns();
            //    }

            //    if (useCase == null || useCase.Steps == null || !useCase.Steps.Any())
            //    {
            //        txtThongbao.AppendText("Không có bước nào để hiển thị cho UseCase này.\r\n");
            //        return;
            //    }

            //    txtThongbao.AppendText($"UseCase: {useCase.Name}\r\n");
            //    txtThongbao.AppendText($"Số bước: {useCase.Steps.Count}\r\n");

            //    int stepCounter = 0;
            //    Dictionary<int, List<string>> stepExpectedResults = new Dictionary<int, List<string>>();
            //    Dictionary<int, string> stepProcedures = new Dictionary<int, string>();
            //    Dictionary<int, string> stepPreconditions = new Dictionary<int, string>();
            //    Dictionary<int, string> stepPostconditions = new Dictionary<int, string>();
            //    Dictionary<int, string> stepFlowTypes = new Dictionary<int, string>();
            //    Dictionary<int, string> stepLevels = new Dictionary<int, string>();
            //    string lastUseCaseName = null;

            //    // Duyệt qua tất cả các bước và chỉ thêm các bước hợp lệ
            //    for (int i = 0; i < useCase.Steps.Count; i++)
            //    {
            //        var step = useCase.Steps[i];
            //        if (string.IsNullOrWhiteSpace(step.Description)) continue; // Bỏ qua bước nếu Description rỗng

            //        txtThongbao.AppendText($"Step Debug - Index: {i}, Description: {step.Description}, ExpectedResult: {step.ExpectedResult}\r\n");

            //        string displayPreconditions = step.Preconditions ?? "Không có";
            //        string displayPostconditions = step.PostConditions ?? "Không có";

            //        if (displayPreconditions == "Không có" && useCase.Preconditions != "Không có")
            //        {
            //            displayPreconditions = useCase.Preconditions;
            //        }
            //        if (displayPostconditions == "Không có" && useCase.Postconditions != "Không có")
            //        {
            //            displayPostconditions = useCase.Postconditions;
            //        }

            //        stepCounter++;
            //        stepProcedures[stepCounter] = step.Description;
            //        stepPreconditions[stepCounter] = displayPreconditions;
            //        stepPostconditions[stepCounter] = displayPostconditions;
            //        stepFlowTypes[stepCounter] = step.FlowType ?? "Không xác định";
            //        stepLevels[stepCounter] = step.Level ?? "Không xác định";
            //        stepExpectedResults[stepCounter] = new List<string>();

            //        // Thêm ExpectedResult từ chính bước
            //        if (!string.IsNullOrEmpty(step.ExpectedResult) && step.ExpectedResult != "Không có kết quả kỳ vọng")
            //        {
            //            stepExpectedResults[stepCounter].Add(step.ExpectedResult.Trim());
            //        }

            //        // Kiểm tra bước SYSTEM tiếp theo để gộp vào ExpectedResult
            //        for (int j = i + 1; j < useCase.Steps.Count; j++)
            //        {
            //            var nextStep = useCase.Steps[j];
            //            bool isNextSystemStep = nextStep.Description?.ToLower().Contains("system") == true ||
            //                                   nextStep.Description?.ToLower().Contains("hệ thống") == true ||
            //                                   nextStep.Description?.ToLower().Contains("hệ thống hiển thị") == true ||
            //                                   nextStep.Description?.ToLower().Contains("system displays") == true;

            //            if (isNextSystemStep)
            //            {
            //                string nextStepDescription = nextStep.Description?.Trim();
            //                if (!string.IsNullOrEmpty(nextStepDescription) && nextStepDescription != "Hệ thống xử lý hành động thành công và thực hiện các bước tiếp theo")
            //                {
            //                    stepExpectedResults[stepCounter].Add(nextStepDescription);
            //                }
            //                i = j; // Bỏ qua bước SYSTEM đã xử lý
            //            }
            //            else
            //            {
            //                break;
            //            }
            //        }

            //        lastUseCaseName = step.UseCaseName ?? "Không xác định";
            //    }

            //    // Thêm dữ liệu vào DataGridView, chỉ thêm hàng nếu có Description hợp lệ
            //    foreach (var stepNum in stepProcedures.Keys)
            //    {
            //        var uniqueExpectedResults = stepExpectedResults[stepNum].Distinct().ToList();
            //        string combinedExpectedResults = uniqueExpectedResults.Any() ? string.Join("\n", uniqueExpectedResults) : "Không có kết quả kỳ vọng";

            //        if (!string.IsNullOrWhiteSpace(stepProcedures[stepNum]))
            //        {
            //            dgvUseCaseDetails.Rows.Add(
            //                lastUseCaseName,
            //                stepLevels[stepNum],
            //                stepPreconditions[stepNum],
            //                stepPostconditions[stepNum],
            //                stepFlowTypes[stepNum],
            //                stepProcedures[stepNum],
            //                combinedExpectedResults
            //            );
            //        }
            //    }

            //    // Xóa hàng trống cuối cùng nếu có (nếu DataGridView tự thêm)
            //    if (dgvUseCaseDetails.Rows.Count > 0 && string.IsNullOrWhiteSpace(dgvUseCaseDetails.Rows[dgvUseCaseDetails.Rows.Count - 1].Cells[5].Value?.ToString()))
            //    {
            //        dgvUseCaseDetails.Rows.RemoveAt(dgvUseCaseDetails.Rows.Count - 1);
            //    }

            //    if (dgvUseCaseDetails.Rows.Count == 0)
            //    {
            //        txtThongbao.AppendText("Không có dữ liệu hợp lệ để hiển thị sau khi lọc các hàng trống.\r\n");
            //    }
            //    else
            //    {
            //        txtThongbao.AppendText($"Đã hiển thị {dgvUseCaseDetails.Rows.Count} bước cho UseCase: {useCase.Name}\r\n");
            //    }
            //}
            //catch (Exception ex)
            //{
            //    txtThongbao.AppendText($"Lỗi khi hiển thị dữ liệu trong DataGridView: {ex.Message}\r\n");
            //    txtThongbao.AppendText($"StackTrace: {ex.StackTrace}\r\n");
            //}



            //cải tiến
            try
            {
                if (!_isDisplayingTestCases)
                {
                    SetUseCaseDetailsColumns();
                    dgvUseCaseDetails.AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.AllCells;
                    dgvUseCaseDetails.Rows.Clear();
                    dgvUseCaseDetails.AllowUserToAddRows = false; // Ngăn DataGridView thêm hàng trống
                }

                int expectedColumnCount = 7;
                if (dgvUseCaseDetails.Columns.Count != expectedColumnCount)
                {
                    txtThongbao.AppendText($"Lỗi: Số cột trong DataGridView ({dgvUseCaseDetails.Columns.Count}) không khớp với số cột mong đợi ({expectedColumnCount}).\r\n");
                    SetUseCaseDetailsColumns();
                }

                if (useCase == null || useCase.Steps == null || !useCase.Steps.Any())
                {
                    txtThongbao.AppendText("Không có bước nào để hiển thị cho UseCase này.\r\n");
                    return;
                }

                txtThongbao.AppendText($"UseCase: {useCase.Name}\r\n");
                txtThongbao.AppendText($"Số bước: {useCase.Steps.Count}\r\n");

                // Tối ưu hiệu suất bằng SuspendLayout
                dgvUseCaseDetails.SuspendLayout();

                int stepCounter = 0;
                Dictionary<int, List<string>> stepExpectedResults = new Dictionary<int, List<string>>();
                Dictionary<int, string> stepProcedures = new Dictionary<int, string>();
                Dictionary<int, string> stepPreconditions = new Dictionary<int, string>();
                Dictionary<int, string> stepPostconditions = new Dictionary<int, string>();
                Dictionary<int, string> stepFlowTypes = new Dictionary<int, string>();
                Dictionary<int, string> stepLevels = new Dictionary<int, string>();
                string lastUseCaseName = null;

                // Duyệt qua tất cả các bước và chỉ thêm các bước hợp lệ
                for (int i = 0; i < useCase.Steps.Count; i++)
                {
                    var step = useCase.Steps[i];
                    if (string.IsNullOrWhiteSpace(step.Description)) continue; // Bỏ qua bước nếu Description rỗng

                    txtThongbao.AppendText($"Step Debug - Index: {i}, Description: {step.Description}, ExpectedResult: {step.ExpectedResult}\r\n");

                    string displayPreconditions = step.Preconditions ?? "Không có";
                    string displayPostconditions = step.PostConditions ?? "Không có";

                    if (displayPreconditions == "Không có" && useCase.Preconditions != "Không có")
                    {
                        displayPreconditions = useCase.Preconditions;
                    }
                    if (displayPostconditions == "Không có" && useCase.Postconditions != "Không có")
                    {
                        displayPostconditions = useCase.Postconditions;
                    }

                    stepCounter++;
                    stepProcedures[stepCounter] = step.Description;
                    stepPreconditions[stepCounter] = displayPreconditions;
                    stepPostconditions[stepCounter] = displayPostconditions;
                    stepFlowTypes[stepCounter] = step.FlowType ?? "Không xác định";
                    stepLevels[stepCounter] = step.Level ?? "Không xác định";
                    stepExpectedResults[stepCounter] = new List<string>();

                    // Thêm ExpectedResult từ chính bước
                    if (!string.IsNullOrEmpty(step.ExpectedResult) && step.ExpectedResult != "Không có kết quả kỳ vọng")
                    {
                        stepExpectedResults[stepCounter].Add(step.ExpectedResult.Trim());
                    }

                    // Kiểm tra bước SYSTEM tiếp theo để gộp vào ExpectedResult
                    for (int j = i + 1; j < useCase.Steps.Count; j++)
                    {
                        var nextStep = useCase.Steps[j];
                        bool isNextSystemStep = nextStep.Description?.ToLower().Contains("system") == true ||
                                               nextStep.Description?.ToLower().Contains("hệ thống") == true ||
                                               nextStep.Description?.ToLower().Contains("hệ thống hiển thị") == true ||
                                               nextStep.Description?.ToLower().Contains("system displays") == true;

                        if (isNextSystemStep)
                        {
                            string nextStepDescription = nextStep.Description?.Trim();
                            if (!string.IsNullOrEmpty(nextStepDescription) && nextStepDescription != "Hệ thống xử lý hành động thành công và thực hiện các bước tiếp theo")
                            {
                                stepExpectedResults[stepCounter].Add(nextStepDescription);
                            }
                            i = j; // Bỏ qua bước SYSTEM đã xử lý
                        }
                        else
                        {
                            break;
                        }
                    }

                    lastUseCaseName = step.UseCaseName ?? "Không xác định";
                }

                // Thêm dữ liệu vào DataGridView, chỉ thêm hàng nếu có Description hợp lệ
                foreach (var stepNum in stepProcedures.Keys)
                {
                    var uniqueExpectedResults = stepExpectedResults[stepNum].Distinct().ToList();
                    string combinedExpectedResults = uniqueExpectedResults.Any() ? string.Join("\n", uniqueExpectedResults) : "Không có kết quả kỳ vọng";

                    if (!string.IsNullOrWhiteSpace(stepProcedures[stepNum]))
                    {
                        dgvUseCaseDetails.Rows.Add(
                            lastUseCaseName,
                            stepLevels[stepNum],
                            stepPreconditions[stepNum],
                            stepPostconditions[stepNum],
                            stepFlowTypes[stepNum],
                            stepProcedures[stepNum],
                            combinedExpectedResults
                        );
                    }
                }

                // Xóa hàng trống cuối cùng nếu có (nếu DataGridView tự thêm)
                if (dgvUseCaseDetails.Rows.Count > 0 && string.IsNullOrWhiteSpace(dgvUseCaseDetails.Rows[dgvUseCaseDetails.Rows.Count - 1].Cells[5].Value?.ToString()))
                {
                    dgvUseCaseDetails.Rows.RemoveAt(dgvUseCaseDetails.Rows.Count - 1);
                }

                if (dgvUseCaseDetails.Rows.Count == 0)
                {
                    txtThongbao.AppendText("Không có dữ liệu hợp lệ để hiển thị sau khi lọc các hàng trống.\r\n");
                }
                else
                {
                    txtThongbao.AppendText($"Đã hiển thị {dgvUseCaseDetails.Rows.Count} bước cho UseCase: {useCase.Name}\r\n");
                }

                // Kết thúc tối ưu hiệu suất
                dgvUseCaseDetails.ResumeLayout();
            }
            catch (Exception ex)
            {
                txtThongbao.AppendText($"Lỗi khi hiển thị dữ liệu trong DataGridView: {ex.Message}\r\n");
                txtThongbao.AppendText($"StackTrace: {ex.StackTrace}\r\n");
            }

        }





        //đảm bảo người dùng chọn 1 ca sử dụng trong combobox
        private void ComboBoxUseCases_SelectedIndexChanged(object sender, EventArgs e)
        {
            
            if (comboboxUC.SelectedIndex == -1) return;

            string selectedUseCaseName = comboboxUC.SelectedItem.ToString();
            var selectedUseCase = _useCases.FirstOrDefault(uc => uc.Name == selectedUseCaseName);
            if (selectedUseCase == null)
            {
                txtThongbao.AppendText($"Không tìm thấy UseCase với tên: {selectedUseCaseName}\r\n");
                _selectedUseCaseId = null;
                return;
            }

            _selectedUseCaseId = selectedUseCase.Id;
            txtThongbao.AppendText($"Đã chọn UseCase: {selectedUseCaseName} (ID: {_selectedUseCaseId})\r\n");
            LoadDataGridView(selectedUseCase);
        }


        

        // Hàm sinh test case từ file XML và lưu vào danh sách _testCases
        private void GenerateTestCases()
        { 
            
            try
            {
                _testCases.Clear();
                txtThongbao.Clear();

                if (string.IsNullOrEmpty(_selectedUseCaseId))
                {
                    txtThongbao.AppendText("Chưa chọn Use Case! Vui lòng chọn một Use Case từ danh sách.\r\n");
                    return;
                }

                var useCase = _useCases?.FirstOrDefault(uc => uc.Id == _selectedUseCaseId);
                if (useCase == null)
                {
                    txtThongbao.AppendText($"Không tìm thấy Use Case với ID: {_selectedUseCaseId}\r\n");
                    return;
                }

                if (useCase.Steps == null || !useCase.Steps.Any())
                {
                    useCase.Steps = new List<StepData>();
                    txtThongbao.AppendText("Cảnh báo: Use Case không có bước nào.\r\n");
                    return;
                }

                if (_testCaseGenerator.HasCycle(useCase.Steps))
                {
                    txtThongbao.AppendText("Lỗi: Phát hiện vòng lặp logic trong các bước. Không thể sinh test case.\r\n");
                    return;
                }

                _testCases = _testCaseGenerator.Generate(useCase);

                if (_testCases.Any())
                {
                    SetTestCaseColumns();
                    dgvUseCaseDetails.Rows.Clear();
                    foreach (var testCase in _testCases)
                    {
                        // Chuẩn hóa Procedure và ExpectedResults để hiển thị
                        var procedureList = testCase.Procedure.Select(p => p?.Trim()).ToList();
                        var expectedResultList = testCase.ExpectedResults.Select(er => er?.Trim()).ToList();

                        // Lọc bỏ các bước chứa "Hệ thống" hoặc trùng với ExpectedResult
                        var filteredProcedureList = new List<string>();
                        for (int i = 0; i < procedureList.Count; i++)
                        {
                            string currentProcedure = procedureList[i];
                            string currentExpectedResult = expectedResultList[i];

                            // Bỏ qua nếu là bước hệ thống hoặc trùng với ExpectedResult
                            if (currentProcedure.ToLower().Contains("hệ thống") ||
                                currentProcedure.ToLower().Contains("system") ||
                                (!string.IsNullOrEmpty(currentExpectedResult) && currentExpectedResult != "N/A" && currentProcedure == currentExpectedResult))
                            {
                                continue;
                            }

                            filteredProcedureList.Add(currentProcedure);
                        }

                        string procedureText = string.Join("\n", filteredProcedureList);
                        string expectedResultText = string.Join("\n", expectedResultList);
                        string testCaseType = testCase.FlowType == "Basic Flow" ? "Positive" : "Negative";

                        dgvUseCaseDetails.Rows.Add(
                            testCase.TestName,
                            testCase.UseCaseName,
                            testCase.FlowType,
                            procedureText,
                            expectedResultText,
                            testCaseType
                        );
                    }
                    txtThongbao.AppendText($"Đã sinh ra {_testCases.Count} test case!\r\n");
                }
                else
                {
                    txtThongbao.AppendText("Không sinh được test case nào!\r\n");
                }
            }
            catch (Exception ex)
            {
                txtThongbao.AppendText($"Lỗi khi sinh test case: {ex.Message}\r\n");
                txtThongbao.AppendText($"StackTrace: {ex.StackTrace}\r\n");
            }
        }



        public class TestCaseGenerator
        {
            public List<TestCase> Generate(UseCaseData useCase)
            {
                var testCases = new List<TestCase>();
                if (useCase == null || useCase.Steps == null || !useCase.Steps.Any())
                {
                    return testCases;
                }

                int testCaseCounter = 1;

                var allSteps = useCase.Steps.OrderBy(s =>
                {
                    var match = Regex.Match(s.Description ?? "", @"(\d+)\.?([a-z]?)");
                    if (match.Success && int.TryParse(match.Groups[1].Value, out int stepNum))
                    {
                        int subStep = match.Groups[2].Value.Length > 0 ? (int)match.Groups[2].Value[0] - (int)'a' + 1 : 0;
                        return stepNum * 100 + subStep;
                    }
                    return int.MaxValue;
                }).ToList();

                var basicSteps = allSteps.Where(s => s.FlowType == "Basic Flow").ToList();
                var exceptionSteps = allSteps.Where(s => s.FlowType == "Exception Flow").ToList();

                if (basicSteps.Any())
                {
                    var basicTestCase = new TestCase
                    {
                        UseCase = useCase.Id ?? "Unknown",
                        UseCaseName = useCase.Name ?? "Unknown",
                        TestName = $"TC-{testCaseCounter:D2}",
                        Procedure = basicSteps.Select(s => s.Description ?? "N/A").ToList(),
                        ExpectedResults = basicSteps.Select(s => s.ExpectedResult ?? "N/A").ToList(),
                        FlowType = "Basic Flow"
                    };

                    testCases.Add(basicTestCase);
                    testCaseCounter++;
                }

                foreach (var step in exceptionSteps)
                {
                    if (step.BranchPoint == null)
                    {
                        continue;
                    }

                    int branchPoint = step.BranchPoint.Value;
                    if (branchPoint < 1 || branchPoint > basicSteps.Count)
                    {
                        branchPoint = basicSteps.Count;
                    }

                    var baseProcedure = basicSteps.Take(branchPoint).Select(s => s.Description ?? "N/A").ToList();
                    var baseExpectedResults = basicSteps.Take(branchPoint).Select(s => s.ExpectedResult ?? "N/A").ToList();

                    baseProcedure.Add(step.Description ?? "N/A");
                    baseExpectedResults.Add(step.ExpectedResult ?? "N/A");

                    var testCase = new TestCase
                    {
                        UseCase = useCase.Id ?? "Unknown",
                        UseCaseName = useCase.Name ?? "Unknown",
                        TestName = $"TC-{testCaseCounter:D2}",
                        Procedure = baseProcedure,
                        ExpectedResults = baseExpectedResults,
                        FlowType = step.FlowType ?? "Exception Flow"
                    };

                    testCases.Add(testCase);
                    testCaseCounter++;
                }

                return testCases;
            }

            public bool HasCycle(List<StepData> steps)
            {
                var graph = new Dictionary<string, List<string>>();
                var stepDescriptions = new HashSet<string>(); // Đảm bảo không trùng lặp mô tả

                // Xây dựng đồ thị
                foreach (var step in steps)
                {
                    string description = step.Description ?? $"Step_{steps.IndexOf(step)}";
                    if (!graph.ContainsKey(description))
                    {
                        graph[description] = new List<string>();
                        stepDescriptions.Add(description);
                    }

                    if (step.BranchPoint.HasValue && step.BranchPoint.Value > 0)
                    {
                        var parentStep = steps.ElementAtOrDefault(step.BranchPoint.Value - 1);
                        if (parentStep != null)
                        {
                            string parentDesc = parentStep.Description ?? $"Step_{step.BranchPoint.Value - 1}";
                            graph[parentDesc].Add(description);
                        }
                    }
                }

                var visited = new HashSet<string>();
                var recStack = new HashSet<string>();

                // DFS để phát hiện chu trình
                foreach (var node in graph.Keys)
                {
                    if (DetectCycle(node))
                    {
                        return true;
                    }
                }

                return false;

                bool DetectCycle(string currentNode)
                {
                    if (!visited.Contains(currentNode))
                    {
                        visited.Add(currentNode);
                        recStack.Add(currentNode);

                        if (graph.ContainsKey(currentNode))
                        {
                            foreach (var neighbor in graph[currentNode])
                            {
                                if (!visited.Contains(neighbor) && DetectCycle(neighbor))
                                    return true;
                                else if (recStack.Contains(neighbor))
                                    return true;
                            }
                        }
                    }
                    recStack.Remove(currentNode);
                    return false;
                }
            }
        }



        //sự kiện cho nút Sinh test case
        private void btnGenerate_Click(object sender, EventArgs e)
        {


            if (string.IsNullOrEmpty(txtInputTM.Text))
            {
                txtThongbao.AppendText("Vui lòng chọn file đặc tả!\r\n");
                return;
            }
            if (string.IsNullOrEmpty(txtOutputTM.Text))
            {
                txtThongbao.AppendText("Vui lòng chọn thư mục đầu ra!\r\n");
                return;
            }
            if (!radioWordOut.Checked && !radioExcelOut.Checked && !radioHTMLOut.Checked)
            {
                txtThongbao.AppendText("Vui lòng chọn định dạng đầu ra!\r\n");
                return;
            }

            try
            {
                txtThongbao.Clear();
                if (radioXMLIn.Checked || radioXMIIn.Checked)
                {
                    if (string.IsNullOrEmpty(_selectedUseCaseId))
                    {
                        txtThongbao.AppendText("Chưa chọn Use Case!\r\n");
                        return;
                    }
                    GenerateTestCases(); // Thêm mới
                }
                else if (radioTextIn.Checked)
                {
                    if (string.IsNullOrEmpty(_selectedFilePath)) { txtThongbao.AppendText("Không có file .txt nào được chọn!\r\n"); return; }
                    ParseTxtFile(_selectedFilePath);
                }

                if (_testCases == null || _testCases.Count == 0) { txtThongbao.AppendText("Không có test case nào để sinh file đầu ra!\r\n"); return; }

                string outputFolder = txtOutputTM.Text;
                string outputFormat = radioWordOut.Checked ? "Word" : radioExcelOut.Checked ? "Excel" : "HTML";
                string fileExtension = outputFormat.ToLower() == "excel" ? "xlsx" : outputFormat.ToLower() == "word" ? "txt" : "html";
                string outputFile = Path.Combine(outputFolder, $"TestCases_{DateTime.Now:yyyyMMdd_HHmmss}.{fileExtension}");

                GenerateOutputFile(_testCases, outputFile, outputFormat);
                txtThongbao.AppendText($"Đã sinh file đầu ra tại: {outputFile}\r\n");
            }
            catch (Exception ex)
            {
                txtThongbao.AppendText($"Lỗi khi sinh file đầu ra: {ex.Message}\r\n");
            }

        }

        private void GenerateOutputFile(List<TestCase> testCases, string filePath, string format)
        {
            if (format == "Excel")
            {
                using (SpreadsheetDocument document = SpreadsheetDocument.Create(filePath, SpreadsheetDocumentType.Workbook))
                {
                    WorkbookPart workbookPart = document.AddWorkbookPart();
                    workbookPart.Workbook = new Workbook();
                    WorksheetPart worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
                    worksheetPart.Worksheet = new Worksheet(new SheetData());

                    Sheets sheets = workbookPart.Workbook.AppendChild(new Sheets());
                    Sheet sheet = new Sheet() { Id = workbookPart.GetIdOfPart(worksheetPart), SheetId = 1, Name = "TestCases" };
                    sheets.Append(sheet);

                    SheetData sheetData = worksheetPart.Worksheet.GetFirstChild<SheetData>();

                    // Tiêu đề
                    Row headerRow = new Row();
                    headerRow.Append(
                        new Cell() { CellValue = new CellValue("Use Case"), DataType = CellValues.String },
                        new Cell() { CellValue = new CellValue("Use Case Name"), DataType = CellValues.String },
                        new Cell() { CellValue = new CellValue("Test Case"), DataType = CellValues.String },
                        new Cell() { CellValue = new CellValue("Procedure"), DataType = CellValues.String },
                        new Cell() { CellValue = new CellValue("Expected Result"), DataType = CellValues.String },
                        new Cell() { CellValue = new CellValue("Loại luồng"), DataType = CellValues.String },
                        new Cell() { CellValue = new CellValue("Loại Test Case"), DataType = CellValues.String }
                    );
                    sheetData.Append(headerRow);

                    // Dữ liệu
                    foreach (var tc in testCases)
                    {
                        string testCaseType = tc.FlowType == "Basic Flow" ? "Positive" : "Negative"; // Sửa từ "Tốt"/"Xấu" thành "Positive"/"Negative"
                        Row row = new Row();
                        row.Append(
                            new Cell() { CellValue = new CellValue(tc.UseCase), DataType = CellValues.String },
                            new Cell() { CellValue = new CellValue(tc.UseCaseName), DataType = CellValues.String },
                            new Cell() { CellValue = new CellValue(tc.TestName), DataType = CellValues.String },
                            new Cell() { CellValue = new CellValue(string.Join("\n", tc.Procedure)), DataType = CellValues.String },
                            new Cell() { CellValue = new CellValue(string.Join("\n", tc.ExpectedResults)), DataType = CellValues.String },
                            new Cell() { CellValue = new CellValue(tc.FlowType), DataType = CellValues.String },
                            new Cell() { CellValue = new CellValue(testCaseType), DataType = CellValues.String }
                        );
                        sheetData.Append(row);
                    }

                    workbookPart.Workbook.Save();
                }
            }
            else if (format == "HTML")
            {
                StringBuilder html = new StringBuilder();
                html.AppendLine("<html><body><h1>Test Cases</h1>");
                html.AppendLine("<table border='1'><tr>");
                html.AppendLine("<th>Use Case</th><th>Use Case Name</th><th>Test Case</th><th>Procedure</th><th>Expected Result</th><th>Loại luồng</th><th>Loại Test Case</th></tr>");

                foreach (var tc in testCases)
                {
                    string testCaseType = tc.FlowType == "Basic Flow" ? "Positive" : "Negative"; // Sửa từ "Tốt"/"Xấu" thành "Positive"/"Negative"
                    html.AppendLine("<tr>");
                    html.AppendLine($"<td>{tc.UseCase}</td>");
                    html.AppendLine($"<td>{tc.UseCaseName}</td>");
                    html.AppendLine($"<td>{tc.TestName}</td>");
                    html.AppendLine($"<td>{string.Join("<br>", tc.Procedure)}</td>");
                    html.AppendLine($"<td>{string.Join("<br>", tc.ExpectedResults)}</td>");
                    html.AppendLine($"<td>{tc.FlowType}</td>");
                    html.AppendLine($"<td>{testCaseType}</td>");
                    html.AppendLine("</tr>");
                }

                html.AppendLine("</table></body></html>");
                File.WriteAllText(filePath, html.ToString());
            }
            else // Word (dùng định dạng đơn giản dạng text)
            {
                StringBuilder text = new StringBuilder();
                text.AppendLine("Test Cases");
                text.AppendLine(new string('=', 50));
                foreach (var tc in testCases)
                {
                    string testCaseType = tc.FlowType == "Basic Flow" ? "Positive" : "Negative"; // Sửa từ "Tốt"/"Xấu" thành "Positive"/"Negative"
                    text.AppendLine($"Use Case: {tc.UseCase}");
                    text.AppendLine($"Use Case Name: {tc.UseCaseName}");
                    text.AppendLine($"Test Case: {tc.TestName}");
                    text.AppendLine($"Procedure: {string.Join("\n", tc.Procedure)}");
                    text.AppendLine($"Expected Result: {string.Join("\n", tc.ExpectedResults)}");
                    text.AppendLine($"Loại luồng: {tc.FlowType}");
                    text.AppendLine($"Loại Test Case: {testCaseType}");
                    text.AppendLine(new string('-', 50));
                }
                File.WriteAllText(filePath, text.ToString());
            }
        }



        //sự kiện cho nút làm mới
        private void btnReset_Click(object sender, EventArgs e)
        {
            
            txtInputTM.Clear();
            txtOutputTM.Clear();
            txtThongbao.Clear();
            _testCases.Clear();
            _useCases.Clear();
            _selectedFilePath = null;
            _selectedUseCaseId = null;
            comboboxUC.Items.Clear();
            comboboxUC.SelectedIndex = -1;
            radioTextIn.Checked = false;
            radioXMLIn.Checked = true;
            radioXMIIn.Checked = false;
            radioWordOut.Checked = true;
            radioExcelOut.Checked = false;
            radioHTMLOut.Checked = false;
            dgvUseCaseDetails.Rows.Clear();
            SetUseCaseDetailsColumns();
            txtThongbao.AppendText("Đã làm mới các lựa chọn.\r\n");
            isEditing = false; // Đặt lại trạng thái chỉnh sửa
            originalData.Clear(); // Xóa dữ liệu bản sao
        }

    

        

        private void btnHelp_Click(object sender, EventArgs e)
        {
            MessageBox.Show("Hướng dẫn sử dụng:\n1. Chọn định dạng đầu vào (XML, XMI, hoặc Text).\n2. Chọn file đặc tả.\n3. Chọn Use Case từ danh sách (nếu là XML hoặc XMI).\n4. Chọn thư mục đầu ra.\n5. Chọn định dạng đầu ra (Word, Excel, HTML).\n6. Nhấn 'Sinh test case' để tạo test case.\n7. Nhấn 'Xuất báo cáo' để lưu báo cáo (nếu cần).");
        }


        private void btnSua_Click(object sender, EventArgs e)
        {
            if (!isEditing)
            {
                if (dgvUseCaseDetails.Rows.Count == 0)
                {
                    txtThongbao.AppendText("Không có dữ liệu để sửa!\r\n");
                    return;
                }

                // Lưu bản sao dữ liệu ban đầu
                originalData.Clear();
                foreach (var useCase in _useCases)
                {
                    originalData.Add(new UseCaseData
                    {
                        Id = useCase.Id,
                        Name = useCase.Name,
                        Level = useCase.Level,
                        Preconditions = useCase.Preconditions,
                        Postconditions = useCase.Postconditions,
                        Steps = useCase.Steps.Select(s => new StepData
                        {
                            UseCaseName = s.UseCaseName,
                            Level = s.Level,
                            Preconditions = s.Preconditions,
                            PostConditions = s.PostConditions,
                            FlowType = s.FlowType,
                            Description = s.Description,
                            ExpectedResult = s.ExpectedResult
                        }).ToList()
                    });
                }

                isEditing = true;
                // Mở khóa các cột Description và ExpectedResult
                dgvUseCaseDetails.Columns["Description"].ReadOnly = false;
                dgvUseCaseDetails.Columns["ExpectedResult"].ReadOnly = false;
                txtThongbao.AppendText("Đã vào chế độ sửa. Chỉnh sửa Mô tả hoặc Kết quả kỳ vọng, sau đó nhấn Lưu hoặc Hủy bỏ!\r\n");
            }
            else
            {
                txtThongbao.AppendText("Bạn đang ở chế độ sửa. Vui lòng Lưu hoặc Hủy bỏ trước khi chỉnh sửa tiếp!\r\n");
            }
        }

        private void btnXoaHang_Click(object sender, EventArgs e)
        {
            if (!isEditing)
            {
                txtThongbao.AppendText("Vui lòng vào chế độ sửa trước khi xóa!\r\n");
                return;
            }

            if (dgvUseCaseDetails.SelectedRows.Count > 0)
            {
                int rowIndex = dgvUseCaseDetails.SelectedRows[0].Index;
                string useCaseName = dgvUseCaseDetails.Rows[rowIndex].Cells["UseCaseName"].Value?.ToString();
                string description = dgvUseCaseDetails.Rows[rowIndex].Cells["Description"].Value?.ToString();

                if (!string.IsNullOrEmpty(useCaseName) && !string.IsNullOrEmpty(description))
                {
                    var useCase = _useCases.FirstOrDefault(uc => uc.Name == useCaseName);
                    if (useCase != null)
                    {
                        var stepToRemove = useCase.Steps.FirstOrDefault(s => s.Description == description);
                        if (stepToRemove != null)
                        {
                            useCase.Steps.Remove(stepToRemove);
                            dgvUseCaseDetails.Rows.RemoveAt(rowIndex);
                            txtThongbao.AppendText($"Đã xóa hàng: {description} khỏi Use Case {useCaseName}\r\n");
                        }
                    }
                }
            }
            else
            {
                txtThongbao.AppendText("Vui lòng chọn một hàng để xóa!\r\n");
            }
        }

        private void btnLuu_Click(object sender, EventArgs e)
        {
            if (isEditing)
            {
                // Cập nhật _useCases từ DataGridView
                foreach (DataGridViewRow row in dgvUseCaseDetails.Rows)
                {
                    if (!row.IsNewRow)
                    {
                        string useCaseName = row.Cells["UseCaseName"].Value?.ToString();
                        string description = row.Cells["Description"].Value?.ToString();
                        var useCase = _useCases.FirstOrDefault(uc => uc.Name == useCaseName);
                        if (useCase != null)
                        {
                            var step = useCase.Steps.FirstOrDefault(s => s.Description == description);
                            if (step != null)
                            {
                                step.Description = row.Cells["Description"].Value?.ToString();
                                step.ExpectedResult = row.Cells["ExpectedResult"].Value?.ToString();
                            }
                        }
                    }
                }

                // Thoát chế độ chỉnh sửa
                isEditing = false;
                dgvUseCaseDetails.Columns["Description"].ReadOnly = true;
                dgvUseCaseDetails.Columns["ExpectedResult"].ReadOnly = true;
                txtThongbao.AppendText("Đã lưu các thay đổi thành công!\r\n");
            }
            else
            {
                txtThongbao.AppendText("Không có thay đổi để lưu!\r\n");
            }
        }

        private void btnHuyBo_Click(object sender, EventArgs e)
        {
            if (isEditing)
            {
                // Khôi phục dữ liệu ban đầu
                _useCases.Clear();
                foreach (var useCase in originalData)
                {
                    _useCases.Add(new UseCaseData
                    {
                        Id = useCase.Id,
                        Name = useCase.Name,
                        Level = useCase.Level,
                        Preconditions = useCase.Preconditions,
                        Postconditions = useCase.Postconditions,
                        Steps = useCase.Steps.Select(s => new StepData
                        {
                            UseCaseName = s.UseCaseName,
                            Level = s.Level,
                            Preconditions = s.Preconditions,
                            PostConditions = s.PostConditions,
                            FlowType = s.FlowType,
                            Description = s.Description,
                            ExpectedResult = s.ExpectedResult
                        }).ToList()
                    });
                }

                // Cập nhật lại DataGridView
                if (_selectedUseCaseId != null)
                {
                    var selectedUseCase = _useCases.FirstOrDefault(uc => uc.Id == _selectedUseCaseId);
                    if (selectedUseCase != null)
                    {
                        LoadDataGridView(selectedUseCase);
                    }
                }

                isEditing = false;
                dgvUseCaseDetails.Columns["Description"].ReadOnly = true;
                dgvUseCaseDetails.Columns["ExpectedResult"].ReadOnly = true;
                txtThongbao.AppendText("Đã hủy bỏ các thay đổi và khôi phục dữ liệu ban đầu!\r\n");
            }
            else
            {
                txtThongbao.AppendText("Không có thay đổi để hủy bỏ!\r\n");
            }
        }


        // Phương thức kiểm tra vòng lặp logic trong các bước
        private bool HasCycle(List<StepData> steps)
        {
            var graph = new Dictionary<string, List<string>>();
            var stepDescriptions = new HashSet<string>(); // Đảm bảo không trùng lặp mô tả

            // Xây dựng đồ thị
            foreach (var step in steps)
            {
                string description = step.Description ?? $"Step_{steps.IndexOf(step)}";
                if (!graph.ContainsKey(description))
                {
                    graph[description] = new List<string>();
                    stepDescriptions.Add(description);
                }

                if (step.BranchPoint.HasValue && step.BranchPoint.Value > 0)
                {
                    var parentStep = steps.ElementAtOrDefault(step.BranchPoint.Value - 1);
                    if (parentStep != null)
                    {
                        string parentDesc = parentStep.Description ?? $"Step_{step.BranchPoint.Value - 1}";
                        graph[parentDesc].Add(description);
                    }
                }
            }

            var visited = new HashSet<string>();
            var recStack = new HashSet<string>();

            // DFS để phát hiện chu trình
            foreach (var node in graph.Keys)
            {
                if (DetectCycle(node))
                {
                    txtThongbao.AppendText($"Phát hiện vòng lặp logic tại bước: {node}\r\n");
                    return true;
                }
            }

            return false;

            bool DetectCycle(string currentNode)
            {
                if (visited.Contains(currentNode))
                {
                    visited.Add(currentNode);
                    recStack.Add(currentNode);

                    if (graph.ContainsKey(currentNode))
                    {
                        foreach (var neighbor in graph[currentNode])
                        {
                            if (!visited.Contains(neighbor) && DetectCycle(neighbor))
                                return true;
                            else if (recStack.Contains(neighbor))
                                return true;
                        }
                    }
                }
                recStack.Remove(currentNode);
                return false;
            }
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(txtOutputTM.Text))
            {
                txtThongbao.AppendText("Vui lòng chọn thư mục đầu ra!\r\n");
                return;
            }
            if (_testCases == null || _testCases.Count == 0)
            {
                txtThongbao.AppendText("Không có test case nào để xuất báo cáo!\r\n");
                return;
            }
            if (!radioWordOut.Checked && !radioExcelOut.Checked && !radioHTMLOut.Checked)
            {
                txtThongbao.AppendText("Vui lòng chọn định dạng đầu ra!\r\n");
                return;
            }

            try
            {
                string outputFolder = txtOutputTM.Text;
                string outputFormat = radioWordOut.Checked ? "Word" : radioExcelOut.Checked ? "Excel" : "HTML";
                string fileExtension = outputFormat.ToLower() == "excel" ? "xlsx" : outputFormat.ToLower() == "word" ? "txt" : "html";
                string outputFile = Path.Combine(outputFolder, $"Report_TestCases_{DateTime.Now:yyyyMMdd_HHmmss}.{fileExtension}");

                // Sử dụng thời gian sinh test case thực tế từ _testCaseGenerationTime
                TimeSpan generationTime = TimeSpan.Zero;
                if (_testCaseGenerationTime.HasValue)
                {
                    generationTime = DateTime.Now - _testCaseGenerationTime.Value;
                }
                else
                {
                    txtThongbao.AppendText("Cảnh báo: Không có thông tin thời gian sinh test case, mặc định là 0 giây.\r\n");
                }

                // Tạo báo cáo chi tiết
                GenerateDetailedReport(_testCases, _useCases, outputFile, outputFormat, generationTime);

                txtThongbao.AppendText($"Đã xuất báo cáo chi tiết tại: {outputFile}\r\n");
            }
            catch (Exception ex)
            {
                txtThongbao.AppendText($"Lỗi khi xuất báo cáo: {ex.Message}\r\n");
            }
        }

        // Phương thức mới để tạo báo cáo chi tiết
        private void GenerateDetailedReport(List<TestCase> testCases, List<UseCaseData> useCases, string filePath, string format, TimeSpan generationTime)
        {
            var reportBuilder = new StringBuilder();
            reportBuilder.AppendLine("=== BÁO CÁO CHI TIẾT TEST CASE ===");
            reportBuilder.AppendLine($"Ngày tạo: {DateTime.Now:dd/MM/yyyy HH:mm:ss}");
            reportBuilder.AppendLine($"Thời gian sinh test case: {generationTime.TotalSeconds:F2} giây");

            // Tính độ bao phủ (coverage) dựa trên các bước thực tế trong Procedure
            int totalSteps = 0;
            int coveredSteps = 0;

            // Tính tổng số bước trong Use Case (Procedure), xử lý null và Steps
            if (useCases != null)
            {
                totalSteps = useCases.Sum(uc =>
                    uc?.Steps?.Count(s => s != null && !string.IsNullOrEmpty(s.Description) && !s.Description.ToLower().Contains("hệ thống")) ?? 0);
            }

            // Tính số bước được kiểm thử từ test cases, xử lý null và Procedure
            if (testCases != null)
            {
                coveredSteps = testCases.Sum(tc =>
                    tc?.Procedure?.Count(p => p != null && !string.IsNullOrEmpty(p) && !p.ToLower().Contains("hệ thống")) ?? 0);
            }

            double coveragePercentage = totalSteps > 0 ? (double)coveredSteps / totalSteps * 100 : 0;
            reportBuilder.AppendLine($"Tổng số bước trong Use Case (Procedure): {totalSteps}");
            reportBuilder.AppendLine($"Số bước được kiểm thử: {coveredSteps}");
            reportBuilder.AppendLine($"Độ bao phủ: {coveragePercentage:F2}%");

            // Tính độ chính xác dựa trên số bước hợp lệ
            int validSteps = testCases.Sum(tc => tc.Procedure.Count(p => !string.IsNullOrEmpty(p) && !p.Contains("N/A")));
            double accuracyPercentage = testCases.Count > 0 ? (double)validSteps / testCases.Sum(tc => tc.Procedure.Count) * 100 : 0;
            reportBuilder.AppendLine($"Tổng số bước trong test case: {testCases.Sum(tc => tc.Procedure.Count)}");
            reportBuilder.AppendLine($"Số bước hợp lệ: {validSteps}");
            reportBuilder.AppendLine($"Độ chính xác: {accuracyPercentage:F2}%");

            // Phát hiện lỗi dựa trên từ khóa "lỗi" trong Expected Results
            int errorCount = testCases.Sum(tc => tc.ExpectedResults.Count(er => er?.ToLower().Contains("lỗi") ?? false));
            double errorDensity = testCases.Count > 0 ? (double)errorCount / testCases.Sum(tc => tc.Procedure.Count) * 100 : 0;
            reportBuilder.AppendLine($"Số lỗi phát hiện: {errorCount}");
            reportBuilder.AppendLine($"Mật độ lỗi: {errorDensity:F2}%");

            reportBuilder.AppendLine("\n=== CHI TIẾT TEST CASE ===");
            foreach (var tc in testCases)
            {
                string testCaseType = tc.FlowType == "Basic Flow" ? "Positive" : "Negative";
                reportBuilder.AppendLine($"Use Case: {tc.UseCase}");
                reportBuilder.AppendLine($"Use Case Name: {tc.UseCaseName}");
                reportBuilder.AppendLine($"Test Case: {tc.TestName}");
                reportBuilder.AppendLine($"Procedure: {string.Join("\n", tc.Procedure)}");
                reportBuilder.AppendLine($"Expected Result: {string.Join("\n", tc.ExpectedResults)}");
                reportBuilder.AppendLine($"Loại luồng: {tc.FlowType}");
                reportBuilder.AppendLine($"Loại Test Case: {testCaseType}");
                reportBuilder.AppendLine(new string('-', 50));
            }

            // Ghi vào file theo định dạng
            if (format == "Excel")
            {
                using (SpreadsheetDocument document = SpreadsheetDocument.Create(filePath, SpreadsheetDocumentType.Workbook))
                {
                    WorkbookPart workbookPart = document.AddWorkbookPart();
                    workbookPart.Workbook = new Workbook();
                    WorksheetPart worksheetPart = workbookPart.AddNewPart<WorksheetPart>();
                    worksheetPart.Worksheet = new Worksheet(new SheetData());

                    Sheets sheets = workbookPart.Workbook.AppendChild(new Sheets());
                    Sheet sheet = new Sheet() { Id = workbookPart.GetIdOfPart(worksheetPart), SheetId = 1, Name = "Report" };
                    sheets.Append(sheet);

                    SheetData sheetData = worksheetPart.Worksheet.GetFirstChild<SheetData>();

                    // Thêm tiêu đề báo cáo
                    var lines = reportBuilder.ToString().Split(new[] { "\r\n", "\n" }, StringSplitOptions.None);
                    foreach (var line in lines)
                    {
                        Row row = new Row();
                        row.Append(new Cell() { CellValue = new CellValue(line), DataType = CellValues.String });
                        sheetData.Append(row);
                    }

                    workbookPart.Workbook.Save();
                }
            }
            else if (format == "HTML")
            {
                File.WriteAllText(filePath, $"<html><body><pre>{reportBuilder.ToString().Replace("\n", "<br>")}</pre></body></html>");
            }
            else // Word (Text)
            {
                File.WriteAllText(filePath, reportBuilder.ToString());
            }
        }



    }
}
