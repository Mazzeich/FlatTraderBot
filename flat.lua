--message('[Script has been started...]')

--Run = true

function isMin()
end


function isMax()
end

-- Вызывается при установлении соединения с сервером  
function OnConnected()
  if (OnConnected()) then
    message('[Connected to the server]')
  else
    message('[Cannot connect to the server]')
  end
end


--------------------------------------------------------------
function main()
  message ('[main() has been invoked...]')
  
  -- DataSource для работы со свечами на графике
  ds, errorDesk = CreateDataSource("QJSIM", "SBER", INTERVAL_M10)
  if ds == nil then
    message('[Connection error]: ' .. errorDesk)
  end
  
  -- Проверка, загрузился ли график
  while (errorDesk == "" or errorDesk == nil) and ds:Size() == 0 do
    sleep(1)
  end
  if ((errorDesk ~= "") and (errorDesk ~= nil)) then 
    message ('[Unable to connect to the chart...]: ' .. errorDesk)
    return 0
  end
  
  -- Ограничиваем количество попыток ожидания получения данных от сервера
  local try_count = 0
  while ((ds:Size() == 0) and (try_count < 1000)) do
    sleep(100)
    try_count = try_count + 1
  end
  
  local currentCandle = ds:Size() -- Текущая свеча
  --local numCandles = ds:getNumCandles("sberprice"); message (numCandles)
  local firstCandleIndex = nil
  local maxCandles = math.min(1000, ds:Size()) -- Максимальное количество свечей не может быть больше общего количества свечей в таблице
  local todaysDay = os.date("%d") -- Текущий день месяца
  
  -- Пока не нашли первую свечу дня либо не проверили все свечи на графике
  while ((firstCandleIndex == nil) and (currentCandle > ds:Size() - maxCandles)) do
      -- Если день этой свечи не совпадает с сегодняшним днём, тогда...
      if tonumber(ds:T(currentCandle).day) ~= todaysDay then
        firstCandleIndex = currentCandle - 1 -- ... = индекс искомой свечи
        message ("Found index: " .. tostring (firstCandleIndex))
      end
      currentCandle = currentCandle - 1
  end
    
  return 0
end

--------------------------------------------------------------
--Run = false