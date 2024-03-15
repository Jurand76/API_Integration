using System;
using System.Collections.Generic;
using System.Xml.Serialization;
using static System.Runtime.InteropServices.JavaScript.JSType;

// model for Azymut books, read from Azymut API. Every group of books are located at "Kartoteka" class.
// Elements contains details about book and prices - buying and for customer

namespace Azymut.Models
{
    [XmlRoot(ElementName = "kartoteki")]
    public class Kartoteki
    {
        [XmlElement(ElementName = "kartoteka")]
        public List<Kartoteka> KartotekaList { get; set; }
    }

    public class Kartoteka
    {
        [XmlElement(ElementName = "id")]
        public string Id { get; set; }

        [XmlElement(ElementName = "tytul")]
        public string Tytul { get; set; }

        [XmlElement(ElementName = "podtytul")]
        public string Podtytul { get; set; }

        [XmlElement(ElementName = "wydawca")]
        public Wydawca Wydawca { get; set; }

        [XmlElement(ElementName = "ean")]
        public string? Ean { get; set; }

        [XmlElement(ElementName = "isbn")]
        public string? Isbn { get; set; }

        [XmlElement(ElementName = "isbn_wersji_papierowej")] 
        public string? IsbnWersjiPapierowej { get; set; }

        [XmlElement(ElementName = "isbn_serii")]
        public string? IsbnSerii { get; set; }

        [XmlElement(ElementName = "issn")]
        public string? Issn { get; set; }

        [XmlElement(ElementName = "rok_wydania")]
        public string? RokWydania { get; set; }

        [XmlElement(ElementName = "miejscowosc")]
        public string? Miejscowosc { get; set; }

        [XmlElement(ElementName = "wydanie")]
        public string? Wydanie { get; set; }

        [XmlElement(ElementName = "tytul_tomu")]
        public string? TytulTomu { get; set; }

        [XmlElement(ElementName = "numer_tomu")]
        public string? NumerTomu { get; set; }

        [XmlElement(ElementName = "liczba_tomow")]
        public string? LiczbaTomow { get; set; }

        [XmlElement(ElementName = "tworcy")]
        public Tworcy? Tworcy { get; set; }

        [XmlElement(ElementName = "jezyki")]
        public Jezyki? Jezyki { get; set; }

        [XmlElement(ElementName = "tematyka")]
        public Tematyka? Tematyka { get; set; }

        [XmlElement(ElementName = "typ")]
        public Typ? Typ { get; set; }

        [XmlElement(ElementName = "atrybuty")]
        public Atrybuty? Atrybuty { get; set; }

        [XmlElement(ElementName = "skany")]
        public Skany? Skany { get; set; }

        [XmlElement(ElementName = "dodatki")]
        public Dodatki? Dodatki { get; set; }

        [XmlElement(ElementName = "opis_krotki")]
        public string? OpisKrotki { get; set; }

        [XmlElement(ElementName = "opis")]
        public string? Opis { get; set; }

        [XmlElement(ElementName = "spis_tresci")]
        public string? SpisTresci { get; set; }

        [XmlElement(ElementName = "gr")]
        public string? Gr { get; set; }

        [XmlElement(ElementName = "issues")]
        public Issues? Issues { get; set; }

        [XmlElement(ElementName = "kategorie_akad")]
        public KategorieAkad? KategorieAkad { get; set; }

        [XmlElement(ElementName = "dystrybucja_do")]
        public DateTime? DystrybucjaDo { get; set; }

        [XmlElement(ElementName = "kartoteka_glowna")]
        public string? KartotekaGlowna { get; set; }

        [XmlElement(ElementName = "numer_rozdzialu")]
        public string? NumerRozdzialu { get; set; }

        [XmlElement(ElementName = "tytul_rozdzialu")]
        public string? TytulRozdzialu { get; set; } 
    }

    public class Wydawca
    {
        [XmlAttribute(AttributeName = "id")]
        public string? Id { get; set; }
        [XmlText]
        public string? Name { get; set; }
    }

    public class Tworcy
    {
        [XmlElement(ElementName = "autorzy")]
        public Autorzy? Autorzy { get; set; }

        [XmlElement(ElementName = "redakcja")]
        public Redakcja? Redakcja { get; set; }

        [XmlElement(ElementName = "tlumacze")]
        public Tlumacze? Tlumacze { get; set; }

        [XmlElement(ElementName = "ilustracje")]
        public Ilustracje? Ilustracje { get; set; }

        [XmlElement(ElementName = "fotografie")]
        public Fotografie? Fotografie { get; set; }

        [XmlElement(ElementName = "lektorzy")]
        public Lektorzy? Lektorzy { get; set; }
    }

    public class Autorzy
    {
        [XmlElement("p")]
        public List<string>? NazwaAutora { get; set; }
    }

    public class Redakcja
    {
        [XmlElement("p")]
        public List<string>? NazwaRedaktora { get; set; }
    }

    public class Tlumacze
    {
        [XmlElement("p")]
        public List<string>? NazwaTlumacza { get; set; }
    }

    public class Ilustracje
    {
        [XmlElement("p")]
        public List<string>? NazwaIlustratora { get; set; }
    }

    public class Fotografie
    {
        [XmlElement("p")]
        public List<string>? NazwaFotografa { get; set; }
    }

    public class Lektorzy
    {
        [XmlElement("p")]
        public List<string>? NazwaLektora { get; set; }
    }

    public class Jezyki
    {
        [XmlElement("p")]
        public List<Jezyk> Jezyk { get; set; }
    }

    public class Jezyk
    {
        [XmlAttribute("id")]
        public string? Id { get; set; }
        [XmlText]
        public string? Nazwa { get; set; }
    }

    public class Tematyka
    {
        [XmlElement("p")]
        public List<Temat> Temat { get; set; }
    }

    public class Temat
    {
        [XmlAttribute("id")]
        public string? Id { get; set; }
        [XmlAttribute("glowna")]
        public string? Glowna { get; set; }
        [XmlText]
        public string? Nazwa { get; set; }
    }

    public class Typ
    {
        [XmlAttribute("kod")]
        public string? Kod { get; set; }
        [XmlText]
        public string? Nazwa { get; set; }
    }

    public class Atrybuty
    {
        [XmlElement(ElementName = "strony")]
        public string? Strony { get; set; }

        [XmlElement(ElementName = "czas")]
        public string? Czas { get; set; }
    }

    public class Skany
    {
        [XmlElement("url")]
        public List<Url> Urls { get; set; }
    }

    public class Dodatki
    {
        [XmlElement("url")]
        public List<Url> Urls { get; set; }
    }

    public class Url
    {
        [XmlAttribute("etag")]
        public string? Etag { get; set; }
        [XmlAttribute("format")]
        public string? Format { get; set; }
        [XmlAttribute("typ")]
        public string? Typ { get; set; }
        [XmlText]
        public string? Link { get; set; }
    }

    public class Issues
    {
        [XmlElement("issue")]
        public List<Issue> IssueList { get; set; }
    }

    public class Issue
    {
        [XmlElement(ElementName = "issue_id")]
        public string? IssueId { get; set; }

        [XmlElement(ElementName = "nazwa")]
        public string? Nazwa { get; set; }

        [XmlElement(ElementName = "cena_det_br")]
        public decimal CenaDetBr { get; set; }

        [XmlElement(ElementName = "cena_zak_br")]
        public decimal CenaZakBr { get; set; }

        [XmlElement(ElementName = "cena_promocyjna")]
        public decimal CenaPromocyjna { get; set; }

        [XmlElement(ElementName = "cena_promocyjna_do")]
        public Date? CenaPromocyjnaDo { get; set; }

        [XmlElement(ElementName = "vat_proc")]
        public VatProc? VatProc { get; set; }

        [XmlElement(ElementName = "atrybuty")]
        public IssueAtrybuty IssueAtrybuty { get; set; }
    }

    public class IssueAtrybuty
    {
        [XmlElement(ElementName = "format")]
        public string Format { get; set; }
    }

    public class VatProc
    {
        [XmlAttribute("pkwiu")]
        public string? Pkwiu { get; set; }
        [XmlText]
        public decimal Procent { get; set; }
    }

    public class KategorieAkad
    {
        [XmlElement("p")]
        public List<KategoriaAkad> Kategorie { get; set; }
    }

    public class KategoriaAkad
    {
        [XmlAttribute("id")]
        public string? Id { get; set; }
        [XmlAttribute("glowna")]
        public string Glowna { get; set; }
        [XmlAttribute("lineage")]
        public string? Lineage { get; set; }
        [XmlText]
        public string? Nazwa { get; set; }
    }
}
