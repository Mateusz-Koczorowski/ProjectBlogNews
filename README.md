Uruchomienie aplikacji

Aplikacja powinna zostać pobrana z platformy Github - https://github.com/Mateusz-Koczorowski/ProjectBlogNews

Uwaga!!!

Najbardziej aktualny kod znajduje się na branchu master oraz develop
Proszę używać .NET w wersji 6 oraz bibliotek w wersji 6!

Pobierz repozytorium korzystając z git clone lub pobierz archiwum ZIP:

Po pobraniu kodu przejdź do folderu w którym znajduje się aplikacja, a następnie uruchom plik ProjectBlogNews.sln
Wymagane jest posiadanie zainstalowanego programu Visual Studio oraz .NET w wersji 6.

Struktura projektu powinna zostać uruchomiona w programie Visual Studio.

Utwórz plik .env (plik ze zmiennymi środowiskowymi - ze względów bezpieczeństwa zawartość tego pliku nie można być przechowywana w repozytorium). Plik powinien znajdować się na wysokości pliku .env.example.

Skopiuj sugerowaną zawartość pliku i wklej w utworzonym pliku .env.

Uwaga!
Aplikacja nie zadziała bez utworzonego i wypełnionego pliku .env!!! Proszę uzupełnić wszystkie pola! Sugerujemy użyć wyżej wymienionej zawartości pliku!


Sugerowana zawartość pliku .env:
Admin_Email=admin@admin.com
Admin_Password=Admin123admin!
Admin_FirstName=Admin
Admin_LastName=Adminsky
Admin_BirthDate=1970-01-01
Admin_RoleName=Admin

Author_Email=author@author.com
Author_Password=Author123author!
Author_FirstName=John
Author_LastName=Smith
Author_BirthDate=1970-01-01
Author_RoleName=Author

Reader_Email=reader@reader.com
Reader_Password=Reader123reader!
Reader_FirstName=Lukas
Reader_LastName=Kowalsky
Reader_BirthDate=1970-01-01
Reader_RoleName=Reader




Następnie otwórz konsolę managera pakietów NuGet:


W konsoli wykonaj polecenia w celu wykonania migracji:

Install-Package Microsoft.AspNetCore.Diagnostics.EntityFrameworkCore
Add-Migration CreateIdentitySchema
Update-Database



7. Po wykonaniu tego kroku, uruchom aplikację przyciskiem Run:



