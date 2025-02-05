'Gadec Engineerings Software (c) 2021
Imports System.Reflection
Imports PdfSharp.Pdf
Imports PdfSharp.Pdf.Advanced
Imports PdfSharp.Pdf.Annotations

''' <summary>
''' <para><see cref="PdfSignatureField"/> represents a digital signature field in a pdf document.</para>
''' </summary>
Friend NotInheritable Class PdfSignatureField
    Inherits PdfAnnotation

    ''' <summary>
    ''' Initializes a new instance of <see cref="PdfSignatureField"/> for the specified document.
    ''' <para><see cref="PdfSignatureField"/> represents a digital signature field in a pdf document.</para>
    ''' </summary>
    ''' <param name="document">The pdf document.</param>
    ''' <param name="page">The zero-based pagenumber to insert the signature field.</param>
    ''' <param name="index">An index number for the signature field.</param>
    ''' <param name="rectangle">The rectangle of the signature field.</param>
    ''' <param name="javaScript">The javascript to link to the signature field.</param>
    Public Sub New(ByVal document As PdfDocument, page As Integer, index As Integer, ByVal rectangle As PdfRectangle, ByVal javaScript As PdfDictionary)
        MyBase.New(document)
        Elements.Add("/FT", New PdfName("/Sig"))
        Elements.Add(Keys.T, New PdfString("Signature" & index))
        Elements.Add("/Ff", New PdfInteger(132))
        Elements.Add("/DR", New PdfDictionary())
        Elements.Add(Keys.Subtype, New PdfName("/Widget"))
        Elements.Add("/P", document.Pages(page))
        Dim signatureDictionary = New PdfDictionary(document)
        signatureDictionary.Elements.Add(Keys.Type, New PdfName("/Sig"))
        signatureDictionary.Elements.Add("/Filter", New PdfName("/Adobe.PPKLite"))
        signatureDictionary.Elements.Add("/SubFilter", New PdfName("/adbe.pkcs7.detached"))
        signatureDictionary.Elements.Add(Keys.M, New PdfDate(DateTime.Now))
        Dim irefTable = Nothing
        For Each field In document.[GetType]().GetRuntimeFields
            If field.Name = "_irefTable" Then irefTable = field.GetValue(document)
        Next
        Dim irefTableAdd = irefTable.[GetType]().GetMethods().Where(Function(m) m.Name = "Add").Skip(1).FirstOrDefault()
        irefTableAdd.Invoke(irefTable, New Object() {signatureDictionary})

        Elements.Add("/A", PdfInternals.GetReference(javaScript))
        Elements.Add("/TU", New PdfString(""))

        Elements.Add("/V", signatureDictionary)
        Elements.Add("/Rect", rectangle)
        Flags = PdfAnnotationFlags.Print
        Opacity = 1
    End Sub

End Class
