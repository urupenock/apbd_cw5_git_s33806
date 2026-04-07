Odwrócenie zależności (DIP):
Stworzyłam interfejsy dla repozytoriów (ICustomerRepository, ISubscriptionPlanRepository) oraz serwisu bilingowego (IBillingService)
Wdrożyłam wstrzykiwanie zależności (Dependency Injection) przez konstruktor. Teraz serwis nie tworzy obiektów sam, lecz otrzymuje je z zewnątrz
Kompatybilność wsteczna (Poor Man's DI):
Dodałam bezparametrowy konstruktor, aby istniejący kod konsumenta (LegacyRenewalAppConsumer) nie przestał działać po wprowadzeniu zmian
Zasada jednej odpowiedzialności (SRP) i wzorzec Strategia:
Podatki: Wyodrębniłam logikę obliczania stawek VAT do TaxService. Koniec z sprawdzaniem krajów wewnątrz głównego serwisu
Rabaty: To była największa część. Stworzyłam DiscountService, który zarządza wszystkimi zniżkami w jednym miejscu
Prowizje: Powstał PaymentFeeService do obliczania kosztów transakcji dla różnych metod płatności 
Struktura projektu:
-Interfaces/ – Kontrakty naszych usług
-Services/ – Implementacje logiki (Tax, Discount, Fee, BillingAdapter)
-Models/ – Klasy pomocnicze (np. DiscountResult do przekazywania kwoty i opisu zniżki)
