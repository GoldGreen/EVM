using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace EVM
{
    public class ViewModel : ReactiveObject
    {
        [Reactive] public double Operand1 { get; set; }
        [Reactive] public double Operand2 { get; set; }
        [Reactive] public double Result { get; set; }

        [Reactive] public string Converted1 { get; set; }
        [Reactive] public string Converted2 { get; set; }
        [Reactive] public string ConvertedResult { get; set; }


        private bool[] bits1 = new bool[16];
        private bool[] bits2 = new bool[16];
        private bool[] resBits = new bool[16];

        [Reactive] public bool OV { get; set; }
        [Reactive] public bool Z { get; set; }

        public ICommand ConCommand { get; }
        public ICommand DivCommand { get; }

        public ViewModel()
        {
            this.ObservableForProperty(x => x.Operand1)
                .Subscribe(arg =>
                {
                    double value = arg.Value;
                    if (value >= 1)
                    {
                        Operand1 = 0.99;
                        return;
                    }

                    if (value <= -1)
                    {
                        Operand1 = -0.99;
                        return;
                    }

                    bits1 = ToBites(value);
                    Converted1 = ToStr(bits1);
                });

            this.ObservableForProperty(x => x.Operand2)
                .Subscribe(arg =>
                {
                    double value = arg.Value;
                    if (value >= 1)
                    {
                        Operand2 = 0.99;
                        return;
                    }

                    if (value <= -1)
                    {
                        Operand2 = -0.99;
                        return;
                    }

                    bits2 = ToBites(value);
                    Converted2 = ToStr(bits2);
                });

            DivCommand = ReactiveCommand.Create
            (
                () =>
                {
                    OV = false;
                    Z = false;
                    resBits = Div(bits1, bits2);
                    Result = ToDouble(resBits);
                    ConvertedResult = ToStr(resBits);
                }
            );

            ConCommand = ReactiveCommand.Create
            (
                () =>
                {
                    OV = false;
                    Z = false;
                    resBits = Con(bits1, bits2);
                    Result = ToDouble(resBits);
                    ConvertedResult = ToStr(resBits);
                }
            );

            Operand1 = 0.3;
            Operand2 = 0.4;
        }


        private static string ToStr(bool[] bites)
        {
            return new(bites.Select(x => x ? '1' : '0').ToArray());
        }

        private static bool[] ToBites(double value)
        {
            bool[] bites = new bool[16];

            bites[0] = value < 0;
            value = Math.Abs(value);

            for (int i = 1; i < 15; i++)
            {
                double p = Math.Pow(2, -i);

                if (value >= p)
                {
                    bites[i] = true;
                    value -= p;
                }
                else
                {
                    bites[i] = false;
                }
            }

            return bites;
        }

        private static double ToDouble(bool[] bits)
        {
            double p = 0;

            for (int i = 1; i < 16; i++)
            {
                if (bits[i])
                {
                    p += Math.Pow(2, -i);
                }
            }

            return bits[0] ? -p : p;
        }

        private bool[] Con(bool[] first, bool[] second)
        {
            bool[] res = new bool[16];

            for (int i = 0; i < 16; i++)
            {
                res[i] = first[i] & second[i];
            }

            Z = IsZero(res);
            return res;
        }

        private static bool[] Add(bool[] first, bool[] second)
        {
            bool[] result = new bool[16];
            bool cash = false;

            for (int i = 15; i >= 0; i--)
            {
                result[i] = first[i] ^ second[i] ^ cash;
                cash = first[i] & cash | second[i] & cash | first[i] & second[i];
            }

            return result;
        }

        private bool[] Sub(bool[] first, bool[] second)
        {
            return Add(first, AddCode(second));
        }

        private bool[] Calculate(bool[] first, bool[] second, bool add = true)
        {
            bool[] firstOp = first.ToArray();
            bool[] secondOp = second.ToArray();

            if (!add)
            {
                secondOp[0] = !secondOp[0];
            }

            if (firstOp[0] == secondOp[0])
            {
                bool[] result = Add(firstOp, secondOp);

                if (result[0])
                {
                    OV = true;
                }
                else
                {
                    result[0] = firstOp[0];
                }

                return result;
            }
            else
            {
                bool[] result = Sub(firstOp, secondOp);

                if (result[0])
                {
                    result[0] = firstOp[0];
                }
                else
                {
                    result = Sub(secondOp, firstOp);
                    result[0] = secondOp[0];
                }

                return result;
            }
        }

        private bool[] Div(bool[] first, bool[] second)
        {
            bool[] firstOp = RightShift(first);
            bool[] secondOp = RightShift(second);

            bool[] res = new bool[16];

            bool digit = firstOp[0] ^ secondOp[0];

            firstOp[0] = false;
            secondOp[0] = false;

            firstOp = Calculate(firstOp, secondOp, false);
            bool w = firstOp[0];

            if (!w)
            {
                OV = true;
            }
            else
            {
                firstOp = LeftShift(firstOp);

                for (int i = 1; i < 16; i++)
                {
                    firstOp = Calculate(firstOp, secondOp, w);
                    w = firstOp[0];

                    if (!w)
                    {
                        res[i] = true;
                    }

                    firstOp = LeftShift(firstOp);
                }
            }

            res[0] = digit;
            Z = IsZero(res);
            return res;
        }

        private static bool[] Reverse(bool[] value)
        {
            return value.Select(x => !x).ToArray();
        }

        private static bool[] LeftShift(bool[] value)
        {
            bool[] res = value.ToArray();

            for (int i = 1; i < 15; i++)
            {
                res[i] = res[i + 1];
            }

            res[15] = false;

            return res;
        }

        private static bool[] RightShift(bool[] value)
        {
            bool[] res = value.ToArray();

            for (int i = 15; i > 1; i--)
            {
                res[i] = res[i - 1];
            }

            res[1] = false;

            return res;
        }

        private static bool IsZero(bool[] value)
        {
            return value.Skip(1).All(x => !x);
        }

        private static bool[] AddCode(bool[] op)
        {
            op = Reverse(op);
            bool[] one = new bool[16];
            one[15] = true;
            op = Add(op, one);

            return op;
        }
    }
}
