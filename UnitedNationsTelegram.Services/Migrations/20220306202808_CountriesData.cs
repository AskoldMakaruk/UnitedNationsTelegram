using Microsoft.EntityFrameworkCore.Migrations;

#nullable disable

namespace UnitedNationsTelegram.Migrations
{
    public partial class CountriesData : Migration
    {
        protected override void Up(MigrationBuilder migrationBuilder)
        {
migrationBuilder.Sql(@"INSERT INTO public.""Countries"" (""Name"", ""EmojiFlag"")
VALUES 
 ('острів вознесіння', '🇦🇨'),
('андора', '🇦🇩'),
('оае', '🇦🇪'),
('афганістан', '🇦🇫'),
('Ангілья', '🇦🇮'),
('Албанія', '🇦🇱'),
('Арменія', '🇦🇲'),
('Ангола', '🇦🇴'),
('Антарктика', '🇦🇶'),
('Аргентина', '🇦🇷'),
('Американське Самоа', '🇦🇸'),
('Австрія', '🇦🇹'),
('Австралія', '🇦🇺'),
('Аруба', '🇦🇼'),
('Азербайджан', '🇦🇿'),
('Барбадос', '🇧🇧'),
('Бангладеш', '🇧🇩'),
('Бельгія', '🇧🇪'),
('Буркіна-Фасо', '🇧🇫'),
('Болгарія', '🇧🇬'),
('Бахрейн', '🇧🇭'),
('Бурунді', '🇧🇮'),
('Бенін', '🇧🇯'),
('Бермуди', '🇧🇲'),
('Бруней', '🇧🇳'),
('Болівія', '🇧🇴'),
('Бразилія', '🇧🇷'),
('Багами', '🇧🇸'),
('Бутан', '🇧🇹'),
('Острів Буве', '🇧🇻'),
('Ботсвана', '🇧🇼'),
('беларусь', '🇧🇾'),
('Беліз', '🇧🇿'),
('Канада', '🇨🇦'),
('Центральноафриканська Республіка', '🇨🇫'),
('Швейцарія', '🇨🇭'),
('Острови Кука', '🇨🇰'),
('Чіл', '🇨🇱'),
('Камерун', '🇨🇲'),
('Китай', '🇨🇳'),
('Колумбія', '🇨🇴'),
('Кліппертон', '🇨🇵'),
('Коста-Рика', '🇨🇷'),
('Куба', '🇨🇺'),
('Кабо-Верде', '🇨🇻'),
('Острів Різдва', '🇨🇽'),
('Кіпр', '🇨🇾'),
('Чехія', '🇨🇿'),
('Німеччина', '🇩🇪'),
('Джибуті', '🇩🇯'),
('Данія', '🇩🇰'),
('Домініка', '🇩🇲'),
('Домініканська Республіка', '🇩🇴'),
('Алгерія', '🇩🇿'),
('Еквадор', '🇪🇨'),
('Естонія', '🇪🇪'),
('Єгипет', '🇪🇬'),
('Західна Сахара', '🇪🇭'),
('Еритрея', '🇪🇷'),
('Іспанія', '🇪🇸'),
('Ефіопія', '🇪🇹'),
('Фінляндія', '🇫🇮'),
('Фіджі', '🇫🇯'),
('Фолкленди', '🇫🇰'),
('Мікронезія', '🇫🇲'),
('Фарери', '🇫🇴'),
('Франція', '🇫🇷'),
('Габон', '🇬🇦'),
('Велика Британія', '🇬🇧'),
('Гренада', '🇬🇩'),
('Грузія', '🇬🇪'),
('Гвіана', '🇬🇫'),
('Гернсі', '🇬🇬'),
('Гана', '🇬🇭'),
('Гібралтар', '🇬🇮'),
('Гренландія', '🇬🇱'),
('Гамбія', '🇬🇲'),
('Гвінея', '🇬🇳'),
('Гваделупа', '🇬🇵'),
('Екваторіальна Гвінея', '🇬🇶'),
('Греція', '🇬🇷'),
('Гватемала', '🇬🇹'),
('Гуам', '🇬🇺'),
('Гаяна', '🇬🇾'),
('Гонконг', '🇭🇰'),
('Гондурас', '🇭🇳'),
('Хорватія', '🇭🇷'),
('Гаїті', '🇭🇹'),
('Угорщина', '🇭🇺'),
('Канарські Острови', '🇮🇨'),
('Індонезія', '🇮🇩'),
('Ірландія', '🇮🇪'),
('Ізраїль', '🇮🇱'),
('Острів чел', '🇮🇲'),
('Індія', '🇮🇳'),
('Британська Територія в Індійському Океані', '🇮🇴'),
('Ірак', '🇮🇶'),
('Іран', '🇮🇷'),
('Ісландія', '🇮🇸'),
('Італія', '🇮🇹'),
('Джерсі', '🇯🇪'),
('Ямайка', '🇯🇲'),
('Йорданія', '🇯🇴'),
('Японія', '🇯🇵'),
('Кенія', '🇰🇪'),
('Киргизстан', '🇰🇬'),
('Камбоджа', '🇰🇭'),
('Kiribati', '🇰🇮'),
('Кірибаті', '🇰🇲'),
('Північна Корея', '🇰🇵'),
('Південа Корея', '🇰🇷'),
('Кувейт', '🇰🇼'),
('Кайманові Острови', '🇰🇾'),
('Казахстан', '🇰🇿'),
('Лаос', '🇱🇦'),
('Лебанон', '🇱🇧'),
('Ліхтенштейн', '🇱🇮'),
('Шрі-Ланка', '🇱🇰'),
('Ліберія', '🇱🇷'),
('Лесото', '🇱🇸'),
('Литва', '🇱🇹'),
('Люксембург', '🇱🇺'),
('Латвія', '🇱🇻'),
('Лівія', '🇱🇾'),
('Марокко', '🇲🇦'),
('Монако', '🇲🇨'),
('Молдова', '🇲🇩'),
('Чорногорія', '🇲🇪'),
('Мадагаскар', '🇲🇬'),
('Маршаллові Острови', '🇲🇭'),
('Македонія', '🇲🇰'),
('Малі', '🇲🇱'),
('Монголія', '🇲🇳'),
('Макао', '🇲🇴'),
('Північні Маріани', '🇲🇵'),
('Мартиніка', '🇲🇶'),
('Мавританія', '🇲🇷'),
('Монтсеррат', '🇲🇸'),
('Мальта', '🇲🇹'),
('Маврикій', '🇲🇺'),
('Мальдіви', '🇲🇻'),
('Малаві', '🇲🇼'),
('Мексика', '🇲🇽'),
('Малазія', '🇲🇾'),
('Мозамбік', '🇲🇿'),
('Намібія', '🇳🇦'),
('Нова Каледонія', '🇳🇨'),
('Нігер', '🇳🇪'),
('Острів Норфолк', '🇳🇫'),
('Нігерія', '🇳🇬'),
('Нікарагуа', '🇳🇮'),
('Нідерланди', '🇳🇱'),
('Норвегія', '🇳🇴'),
('Непал', '🇳🇵'),
('Науру', '🇳🇷'),
('Ніуе', '🇳🇺'),
('Нова Зеландія', '🇳🇿'),
('Оман', '🇴🇲'),
('Панама', '🇵🇦'),
('Перу', '🇵🇪'),
('Французька Полінезія', '🇵🇫'),
('Папуа Нова Гвінея', '🇵🇬'),
('Філіппіни', '🇵🇭'),
('Пакістан', '🇵🇰'),
('Польща', '🇵🇱'),
('Піткерн', '🇵🇳'),
('Пуерто-Рико', '🇵🇷'),
('Португалія', '🇵🇹'),
('Палау', '🇵🇼'),
('Парагвай', '🇵🇾'),
('Катар', '🇶🇦'),
('Румунія', '🇷🇴'),
('Сербія', '🇷🇸'),
('росія', '🇷🇺'),
('Руанда', '🇷🇼'),
('Саудівська Аравія', '🇸🇦'),
('Соломонові Острови', '🇸🇧'),
('Сейшельські Острови', '🇸🇨'),
('Судан', '🇸🇩'),
('Швеція', '🇸🇪'),
('Сінгапур', '🇸🇬'),
('Словенія', '🇸🇮'),
('Словакія', '🇸🇰'),
('Сьєрра-Леоне', '🇸🇱'),
('Сан-Марино', '🇸🇲'),
('Сенегал', '🇸🇳'),
('Сомалі', '🇸🇴'),
('Суринам', '🇸🇷'),
('Південний Судан', '🇸🇸'),
('Сальвадор', '🇸🇻'),
('Сінт-Мартен', '🇸🇽'),
('Сирія', '🇸🇾'),
('Острови Святої Єлени, Вознесіння і Тристан-да-Кунья', '🇹🇦'),
('Чад', '🇹🇩'),
('Французькі Південні і Антарктичні Території', '🇹🇫'),
('Того', '🇹🇬'),
('Тайланд', '🇹🇭'),
('Таджикістан', '🇹🇯'),
('Токелау', '🇹🇰'),
('Туркменістан', '🇹🇲'),
('Туніс', '🇹🇳'),
('Тонга', '🇹🇴'),
('Турція', '🇹🇷'),
('Тувалу', '🇹🇻'),
('Тайвань', '🇹🇼'),
('Танзанія', '🇹🇿'),
('Україна', '🇺🇦'),
('Уганда', '🇺🇬'),
('ООН', '🇺🇳'),
('США', '🇺🇸'),
('Уругвай', '🇺🇾'),
('Узбекістан', '🇺🇿'),
('Ватикан', '🇻🇦'),
('Венесуела', '🇻🇪'),
('Британські Віргіни', '🇻🇬'),
(E'В\'єтнам', '🇻🇳'),
('Вануату', '🇻🇺'),
('Самоа', '🇼🇸'),
('Косово', '🇽🇰'),
('Ємен', '🇾🇪'),
('Майотта', '🇾🇹'),
('Південно-Африканська Республіка', '🇿🇦'),
('Замбія', '🇿🇲'),
('Зімбабве', '🇿🇼')
ON CONFLICT DO NOTHING;
");
        }

        protected override void Down(MigrationBuilder migrationBuilder)
        {

        }
    }
}
