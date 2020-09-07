Settings={}
Settings.Name = "candleIndexer";
Settings['Идентификатор'] = "sberprice";
Settings['Период'] = 1;
Settings['Шагов цены вверх'] = 2;

function Init()
    return 1
end
 
function OnCalculate(index)
   if index == 1.0 then
      LastIndex = 1
      DelAllLabels(Settings['Идентификатор'])
      info = getDataSourceInfo()
      price_step = getParamEx("QJSIM", "SBER", 'SEC_PRICE_STEP').param_value
   end
   
	if index - LastIndex < Settings['Период'] then return; else LastIndex = index; end;
   local Date = tonumber(T(index).year);
   local month = tostring(T(index).month);
   if #month == 1 then Date = Date.."0"..month; else Date = Date..month; end;
   local day = tostring(T(index).day);
   if #day == 1 then Date = Date.."0"..day; else Date = Date..day; end;
   Date = tonumber(Date);
   local Time = "";
   local hour = tostring(T(index).hour);
   if #hour == 1 then Time = Time.."0"..hour; else Time = Time..hour; end;
   local minute = tostring(T(index).min);
   if #minute == 1 then Time = Time.."0"..minute; else Time = Time..minute; end;
   local sec = tostring(T(index).sec);
   if #sec == 1 then Time = Time.."0"..sec; else Time = Time..sec; end;
   Time = tonumber(Time);

   local label_params = 
   {
      ['TEXT'] = tostring(index), -- STRING Подпись метки (если подпись не требуется, то пустая строка)  
      ['IMAGE_PATH'] = '', -- STRING Путь к картинке, которая будет отображаться в качестве метки (пустая строка, если картинка не требуется)  
      ['ALIGNMENT'] = 'BOTTOM', -- STRING Расположение картинки относительно текста (возможно 4 варианта: LEFT, RIGHT, TOP, BOTTOM)    
      ['YVALUE'] = H(index) + price_step*Settings['Шагов цены вверх'], -- DOUBLE Значение параметра на оси Y, к которому будет привязана метка  
      ['DATE'] = Date, -- DOUBLE Дата в формате «ГГГГММДД», к которой привязана метка  
      ['TIME'] = Time, -- DOUBLE Время в формате «ЧЧММСС», к которому будет привязана метка  
      ['R'] = 255, -- DOUBLE Красная компонента цвета в формате RGB. Число в интервале [0;255]  
      ['G'] = 255, -- DOUBLE Зеленая компонента цвета в формате RGB. Число в интервале [0;255]  
      ['B'] = 255, -- DOUBLE Синяя компонента цвета в формате RGB. Число в интервале [0;255]  
      ['TRANSPARENCY'] = 0, -- DOUBLE Прозрачность метки в процентах. Значение должно быть в промежутке [0; 100]  
      ['TRANSPARENT_BACKGROUND'] = 1, -- DOUBLE Прозрачность метки. Возможные значения: «0» – прозрачность отключена, «1» – прозрачность включена  
      ['FONT_FACE_NAME'] = 'Arial', -- STRING Название шрифта (например «Arial»)  
      ['FONT_HEIGHT'] = 6, -- DOUBLE Размер шрифта  
      ['HINT'] = '' -- STRING Текст подсказки 
   }
   local label_id = AddLabel(Settings['Идентификатор'], label_params);
end

function OnDestroy()
   DelAllLabels(Settings['Идентификатор']);
end;