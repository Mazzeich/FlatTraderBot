Settings = {}
Settings.Name = "dataParser"
Settings['idChart'] = "mtschart"
Settings['class_code'] = "TQBR" -- QJSIM/TQBR SPBFUT CETS
Settings['sec_code'] = "MTSS"

function main()

  local openPath 	 = "../Lua/Data/dataOpen.txt"
  local closePath  = "../Lua/Data/dataClose.txt"
  local volumePath = "../Lua/Data/dataVolume.txt"
  local highPath 	 = "../Lua/Data/dataHigh.txt"
  local lowPath 	 = "../Lua/Data/dataLow.txt"
  local avgPath    = "../Lua/Data/dataAvg.txt"
  
  local openIO 	 = io.open(openPath  , "w")
  local highIO 	 = io.open(highPath  , "w")
  local lowIO 	 = io.open(lowPath   , "w")
  local closeIO  = io.open(closePath , "w")
  local volumeIO = io.open(volumePath, "w")
  local avgIO    = io.open(avgPath   , "w")

  ds, errorDesk = CreateDataSource(Settings['class_code'], Settings['sec_code'], INTERVAL_M1)
  local tag = Settings['idChart']
  if ds == nil then
    message('[DataParser]Connection error: ' .. errorDesk)
  end

  while (errorDesk == "" or errorDesk == nil) and ds:Size() == 0 do
    sleep(1)
  end
  if ((errorDesk ~= "") and (errorDesk ~= nil)) then 
    message ('[DataParser]: Unable to connect to the chart: ' .. errorDesk)
    return 0
  end

  local try_count = 0
  while ((ds:Size() == 0) and (try_count < 1000)) do
    sleep(100)
    try_count = try_count + 1
  end

  local firstCandleIndex = nil
  local maxCandles = math.min(1000, ds:Size())
  local tLines = getLinesCount(tag)
  local candlesTotal = getNumCandles(tag)
  -- Количетство просматриваемых свечей
  local coveredCandles = 120

  tableCandle, n, lgnd = getCandlesByIndex(tag, 0, 0, candlesTotal)

  for i = n - coveredCandles, n - 1 do
  	local dateCandle = tostring(tableCandle[i].datetime.hour)..":"..tostring(tableCandle[i].datetime.min)

  	openIO:write(tableCandle[i].open.."\n")-- 	 .."\t["..i.."]\t["..dateCandle.."]\n")
  	highIO:write(tableCandle[i].high.."\n")-- 	 .."\t["..i.."]\t["..dateCandle.."]\n")
  	lowIO:write(tableCandle[i].low.."\n")-- 		 .."\t["..i.."]\t["..dateCandle.."]\n")
  	closeIO:write(tableCandle[i].close.."\n")-- 	 .."\t["..i.."]\t["..dateCandle.."]\n")
    volumeIO:write(tableCandle[i].volume.."\n")-- .."\t["..i.."]\t["..dateCandle.."]\n")
    avgIO:write((tableCandle[i].high + tableCandle[i].low)*0.5 .."\n")
  end

  --f:write(data)

  openIO:close()
  highIO:close()
  lowIO:close()
  closeIO:close()
  volumeIO:close()
  avgIO:close()
end