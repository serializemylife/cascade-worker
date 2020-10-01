using OpenQA.Selenium;
using OpenQA.Selenium.Support.UI;
using System;

namespace CascadeWorker.Shared.Extentions
{
    public static class WebDriverExtentions
    {
        public static IWebElement WaitUntilVisible(this IWebDriver driver, By itemSpecifier, int secondsTimeout = 10)
        {
            var wait = new WebDriverWait(driver, new TimeSpan(0, 0, secondsTimeout));

            try
            {
                var element = wait.Until(driver =>
                {
                    try
                    {
                        var elementToBeDisplayed = driver.FindElement(itemSpecifier);

                        if (elementToBeDisplayed.Displayed)
                        {
                            return elementToBeDisplayed;
                        }

                        return null;
                    }
                    catch (StaleElementReferenceException)
                    {
                        return null;
                    }
                    catch (NoSuchElementException)
                    {
                        return null;
                    }

                });

                return element;
            }
            catch (WebDriverTimeoutException)
            {
                return null;
            }
        }
    }
}
