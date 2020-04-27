using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ConsoleApp1
{
    class Program
    {
        static void Main()
        {
            //Количество особей, кратное 4
            int CountIndivid = 100;

            //Максимально значение параметра
            int MaxParameterValue = 10;

            //Количество параметров
            int CountParameter = 30;

            //Вероятность мутации, %
            int ProbabilityMatation = 80;

            //Максимально значение мутации
            int MaxMutationValue = 2;

            //Случайная величина
            Random rnd = new Random();

            //Лист с параметрами, к которым стремятся индивиды, заполняется случайно
            List<int> TargetParameters = NewRandomParameters(MaxParameterValue, CountParameter ,rnd);

            //Старт ГА
            Population.Start(CountIndivid, MaxParameterValue, CountParameter, TargetParameters, ProbabilityMatation, MaxMutationValue, rnd);
            
            //Ожидание завершения работы кода
            Console.ReadKey();
        }

        //Вывод искомых параметров
        public static string TargetParametersToString(List<int> parameters)
        {
            string ret = "Искомые параметры:\n";
            foreach (var a in parameters)
            {
                ret = ret + Convert.ToString(a)+" ";
            }
            ret += "\n\n";
            return ret;
        }

        //Метод заполнения параметров случайными значениями
        public static List<int> NewRandomParameters(int Max, int count, Random rnd)
        {
            List<int> list = new List<int>();
            for (int i = 0; i < count; i++)
            {
                list.Add(rnd.Next(Max));
            }
            return list;
        }
    }

    class Population
    {
        //Главный метод ГА
        public static void Start(
            int CountIndivid,
            int MaxParameterValue, 
            int CountParameter, 
            List<int> TargetParameters,
            int ProbabilityMutation,
            int MaxMutationValue,
            Random rnd)
        {
            //Создание новой популяции
            List<Individ> individs = Population.CreateNewPopulation(CountIndivid,MaxParameterValue,CountParameter,rnd);

            int NumberIteration = 0;

            bool end = false;

            do
            {
                //Вывод искомых параметров
                Console.WriteLine(Program.TargetParametersToString(TargetParameters));

                Console.WriteLine("Итерация номер - " + NumberIteration+"\n");

                end = Check(individs, TargetParameters, CountIndivid, CountParameter);

                Iteration(CountIndivid,
                MaxParameterValue,
                CountParameter,
                TargetParameters,
                ProbabilityMutation,
                MaxMutationValue,
                rnd,
                individs);

                NumberIteration++;
            }
            while (end == false);
            
        }

        //Метод одной итерации ГА
        public static void Iteration(
            int CountIndivid,
            int MaxParameterValue,
            int CountParameter,
            List<int> TargetParameters,
            int ProbabilityMutation,
            int MaxMutationValue,
            Random rnd,
            List<Individ> individs)
        {
            //Селекция по пригодности(остается 4 лучших особи)
            Individ.Select(individs, CountIndivid, MaxParameterValue, CountIndivid, rnd);

            //Скрещивание
            //Выбираю случайные гены от родителей и вставляю потомкам
            //смешиваю 1 и 2, 2 и 4, 1 и 3, 2 и 4 родителя, каждая пара дает 1/4 всей популяции
            Individ.ReplicateAll(individs, MaxParameterValue, CountParameter, CountIndivid, rnd);

            //Мутация
            //Вероятность мутации индивидв и гена одинакова
            //При захождении мутации за край значения параметра она возвращается с другой стороны, значения циклично.
            MutationAll(individs, ProbabilityMutation, MaxMutationValue, CountParameter, MaxParameterValue, rnd);
        }

        //Метод вывода информации о поколении 
        public static string Info(List<Individ> individs)
        {
            string ret = "";

            foreach (var a in individs)
            {
                ret+=a.ToString()+"\n";
            }
            ret += "\n\n";
            return ret;
        }

        //Метод создания новой популяции
        public static List<Individ> CreateNewPopulation(int CountIndivid, int MaxParameterValue, int CountParameter, Random rnd)
        {
            //Создание листа с индивидами
            List<Individ> individs = new List<Individ>();

            for (int i = 0; i < CountIndivid; i++)
            {
                individs.Add(Individ.NewRandomIndivid(MaxParameterValue, CountParameter, rnd));
            }
            return individs;
        }

        //Метод подсчета пригодности всей популяции
        public static void CalcDiffAll(List<Individ> individs, List<int> TargetParameters, int CountParameters, int CountIndivid)
        {
            for (int i = 0; i < CountIndivid; i++)
            {
                individs[i].CalcDiff(TargetParameters,CountParameters);
            }
        }

        //Метод сортировки по пригодности (чем меньше, тем лучше)
        public static void Sort(List<Individ> individs, int CountIndivid)
        {
            Individ temp;
            for (int x = 0; x < CountIndivid; x++)
            {
                for (int y = 0; y < CountIndivid; y++)
                {
                    if (individs[x].diff < individs[y].diff)
                    {
                        temp = individs[x];
                        individs[x] = individs[y];
                        individs[y] = temp;
                    }
                }
            }
        }

        //Метод мутации всех особей
        public static void MutationAll(List<Individ> individs, int Probability,int MaxMutationValue,int CountParameters,int MaxParameterValue,Random rnd)
        {
            foreach(var a in individs)
            {
                a.Mutation(Probability, MaxMutationValue, CountParameters, MaxParameterValue, rnd);
            }
        }

        //Проверка на выполнение задачи
        public static bool Check(List<Individ> individs, List<int> TargetParameters, int CountIndivid, int CountParameters)
        {
            //Подсчет пригодности всей популяции
            CalcDiffAll(individs, TargetParameters, CountParameters, CountIndivid);

            //Сортировка по пригодности(чем меньше значение, тем лучше и выше место в списке)
            Sort(individs, CountIndivid);

            //Вывод информации о популяции
            Console.WriteLine(Info(individs));

            if (individs[0].diff == 0)
                return true;
            else
                return false;
        }
}

    class Individ
    {
        //Значение пригодности(чем оно меньше, тем больше совпадает с искомым, тем лучше)
        public int diff;

        //Метод вычисления пригодности
        public void CalcDiff(List<int> Targetparameters, int CountParameters)
        {
            this.diff = 0;
            for(int i =0;i<CountParameters;i++)
            {
                if(this.Parameters[i]> Targetparameters[i])
                {
                    diff += this.Parameters[i] - Targetparameters[i];
                }
                else
                {
                    diff += Targetparameters[i] - this.Parameters[i];
                }
                
            }
        }

        //Метод селекции
        public static void Select(List<Individ> individs, int CountIndivid, int MaxParameterValue, int CountParameter, Random rnd)
        {
            Individ par1 = individs[0];
            Individ par2 = individs[1];
            Individ par3 = individs[2];
            Individ par4 = individs[3];

            individs.RemoveAll(x => x is Individ);

            individs.Add(par1);
            individs.Add(par2);
            individs.Add(par3);
            individs.Add(par4);
        }

        //Метод скрещивания всех родителей 
        public static void ReplicateAll(List<Individ> individs, int MaxParameterValue, int CountParameter, int CountIndivid, Random rnd)
        {
            Individ.Replicate(individs, individs[0], individs[1], MaxParameterValue, CountParameter, CountIndivid / 4, rnd);
            Individ.Replicate(individs, individs[2], individs[3], MaxParameterValue, CountParameter, CountIndivid / 4, rnd);
            Individ.Replicate(individs, individs[0], individs[2], MaxParameterValue, CountParameter, CountIndivid / 4, rnd);
            Individ.Replicate(individs, individs[1], individs[3], MaxParameterValue, CountParameter, CountIndivid / 4, rnd);
            individs.Remove(individs[0]);
            individs.Remove(individs[0]);
            individs.Remove(individs[0]);
            individs.Remove(individs[0]);
        }

        //Метод скрещивания двух индивидов
        public static void Replicate(List<Individ> individs, Individ par1, Individ par2,int MaxParameterValue, int CountParameter, int CountChild, Random rnd)
        {
            for(int i =0;i<CountChild;i++)
            {
                individs.Add(Individ.Mix(par1, par2,MaxParameterValue,CountParameter,rnd));
            }
        }

        //Метод смешения генов
        public static Individ Mix(Individ par1, Individ par2,int MaxParameterValue,int CountParameter, Random rnd)
        {
            Individ NewInd=Individ.NewRandomIndivid(MaxParameterValue,CountParameter,rnd);

            for(int i = 0; i<CountParameter;i++)
            {
                if(rnd.Next(2)==0)
                {
                    NewInd.Parameters[i] = par1.Parameters[i];
                }
                else
                {
                    NewInd.Parameters[i] = par2.Parameters[i];
                }
            }
            return NewInd;
        }

        //Метод создания нового случайного индивида
        public static Individ NewRandomIndivid(int maxParameter, int countParameter, Random rnd)
        {
            Individ ind = new Individ();

            ind.Parameters = Program.NewRandomParameters(maxParameter, countParameter, rnd);

            return ind;
        }

        //Лист с параметрами
        List<int> Parameters;

        //Метод для вывода информации
        public override string ToString()
        {
            string ret = "";
            foreach(var a in this.Parameters)
            {
                if(a==10)
                {

                }
                ret = ret + Convert.ToString(a)+" ";
            }
            ret += " " + this.diff;
            return ret;
        }

        //Метод мутации
        public void Mutation(int prob, int max,int CountParameters,int MaxParameterValue, Random rnd)
        {
            if(max>=MaxParameterValue)
            {
                max = MaxParameterValue - max;
            }

            //Мутация индивида
            if(rnd.Next(100)<prob)
            {
                for(int i=0;i<CountParameters;i++)
                {
                    //Мутация гена
                    if(rnd.Next(100) < prob)
                    {
                        //Мутация в одну сторону
                        if(rnd.Next(2)==0)
                        {
                            this.Parameters[i] += rnd.Next(max);
                            if(this.Parameters[i]>=MaxParameterValue)
                            {
                                this.Parameters[i] = this.Parameters[i] - MaxParameterValue;
                            }
                        }
                        //в другую
                        else
                        {
                            this.Parameters[i] -= rnd.Next(max);
                            if (this.Parameters[i] < 0)
                            {
                                this.Parameters[i] = MaxParameterValue - Math.Abs(this.Parameters[i]);
                            }
                        }
                    }
                }
            }
        }
    }
}
