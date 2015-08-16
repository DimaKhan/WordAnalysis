using System;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using MorfologAnalysis;

namespace MorfologAnalysisTests
{
    [TestClass]
    public class MorfologAnalysisTest
    {
        [TestMethod]
        public void getMorfologData()
        {
            WordAnalysis wa = WordAnalysis.GetInstance();
            string expected = "папа - начальная форма \"папа\",сущ,од,мр,ед,ип\n" +
                              "мама - начальная форма \"мама\",сущ,од,жр,ед,ип\n" +
                              "и - союз\n" +
                              "я - начальная форма \"я\",мест,личн,1л,ед,ип\n" +
                              "мыли - начальная форма \"мыть\",гл,нв,пе,пв,мн\n" +
                              "раму - начальная форма \"рама\",сущ,не,жр,ед,вп\n" +
                              "новыми - слово не найдено\n" +
                              "тряпками - начальная форма \"тряпка\",сущ,не,жр,мн,тп\n";
            var actual = wa.stringAnalysis("Папа, мама и я мыли раму новыми тряпками!", false);
            Assert.AreEqual(expected, actual);
            
        }

        [TestMethod]
        public void setMorfologData()
        {
            WordAnalysis wa = WordAnalysis.GetInstance();
            string expected = "тряпками";
            var actual = wa.stringAnalysis("тряпке", true, "мн,тп");
            Assert.AreEqual(expected, actual);

        }
    }
}
