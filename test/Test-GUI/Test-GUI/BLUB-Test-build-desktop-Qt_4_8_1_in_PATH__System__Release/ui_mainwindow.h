/********************************************************************************
** Form generated from reading UI file 'mainwindow.ui'
**
** Created: Sat Oct 19 17:48:19 2013
**      by: Qt User Interface Compiler version 4.8.1
**
** WARNING! All changes made in this file will be lost when recompiling UI file!
********************************************************************************/

#ifndef UI_MAINWINDOW_H
#define UI_MAINWINDOW_H

#include <QtCore/QVariant>
#include <QtGui/QAction>
#include <QtGui/QApplication>
#include <QtGui/QButtonGroup>
#include <QtGui/QCheckBox>
#include <QtGui/QComboBox>
#include <QtGui/QHBoxLayout>
#include <QtGui/QHeaderView>
#include <QtGui/QLabel>
#include <QtGui/QLineEdit>
#include <QtGui/QMainWindow>
#include <QtGui/QMenuBar>
#include <QtGui/QPushButton>
#include <QtGui/QSlider>
#include <QtGui/QStatusBar>
#include <QtGui/QToolBar>
#include <QtGui/QVBoxLayout>
#include <QtGui/QWidget>

QT_BEGIN_NAMESPACE

class Ui_MainWindow
{
public:
    QWidget *centralWidget;
    QVBoxLayout *verticalLayout;
    QHBoxLayout *horizontalLayout_2;
    QLabel *label;
    QComboBox *boxPort;
    QPushButton *btnBubble;
    QHBoxLayout *horizontalLayout;
    QLabel *label_2;
    QSlider *sliderFreq;
    QLineEdit *lineEditFreq;
    QCheckBox *chkBoxEnAuto;
    QHBoxLayout *horizontalLayout_3;
    QLabel *label_3;
    QLineEdit *lineEditImageFn;
    QPushButton *btnSelectFile;
    QMenuBar *menuBar;
    QToolBar *mainToolBar;
    QStatusBar *statusBar;
    QToolBar *toolBar;
    QToolBar *toolBar_2;

    void setupUi(QMainWindow *MainWindow)
    {
        if (MainWindow->objectName().isEmpty())
            MainWindow->setObjectName(QString::fromUtf8("MainWindow"));
        MainWindow->resize(581, 228);
        centralWidget = new QWidget(MainWindow);
        centralWidget->setObjectName(QString::fromUtf8("centralWidget"));
        verticalLayout = new QVBoxLayout(centralWidget);
        verticalLayout->setSpacing(6);
        verticalLayout->setContentsMargins(11, 11, 11, 11);
        verticalLayout->setObjectName(QString::fromUtf8("verticalLayout"));
        horizontalLayout_2 = new QHBoxLayout();
        horizontalLayout_2->setSpacing(6);
        horizontalLayout_2->setObjectName(QString::fromUtf8("horizontalLayout_2"));
        label = new QLabel(centralWidget);
        label->setObjectName(QString::fromUtf8("label"));

        horizontalLayout_2->addWidget(label);

        boxPort = new QComboBox(centralWidget);
        boxPort->setObjectName(QString::fromUtf8("boxPort"));

        horizontalLayout_2->addWidget(boxPort);


        verticalLayout->addLayout(horizontalLayout_2);

        btnBubble = new QPushButton(centralWidget);
        btnBubble->setObjectName(QString::fromUtf8("btnBubble"));
        btnBubble->setEnabled(false);

        verticalLayout->addWidget(btnBubble);

        horizontalLayout = new QHBoxLayout();
        horizontalLayout->setSpacing(6);
        horizontalLayout->setObjectName(QString::fromUtf8("horizontalLayout"));
        label_2 = new QLabel(centralWidget);
        label_2->setObjectName(QString::fromUtf8("label_2"));

        horizontalLayout->addWidget(label_2);

        sliderFreq = new QSlider(centralWidget);
        sliderFreq->setObjectName(QString::fromUtf8("sliderFreq"));
        sliderFreq->setMinimum(150);
        sliderFreq->setMaximum(2000);
        sliderFreq->setSingleStep(10);
        sliderFreq->setPageStep(100);
        sliderFreq->setValue(1000);
        sliderFreq->setOrientation(Qt::Horizontal);

        horizontalLayout->addWidget(sliderFreq);

        lineEditFreq = new QLineEdit(centralWidget);
        lineEditFreq->setObjectName(QString::fromUtf8("lineEditFreq"));
        QSizePolicy sizePolicy(QSizePolicy::Fixed, QSizePolicy::Fixed);
        sizePolicy.setHorizontalStretch(0);
        sizePolicy.setVerticalStretch(0);
        sizePolicy.setHeightForWidth(lineEditFreq->sizePolicy().hasHeightForWidth());
        lineEditFreq->setSizePolicy(sizePolicy);
        lineEditFreq->setMinimumSize(QSize(120, 0));
        lineEditFreq->setMaximumSize(QSize(150, 16777215));
        lineEditFreq->setBaseSize(QSize(120, 0));
        lineEditFreq->setReadOnly(true);

        horizontalLayout->addWidget(lineEditFreq);

        chkBoxEnAuto = new QCheckBox(centralWidget);
        chkBoxEnAuto->setObjectName(QString::fromUtf8("chkBoxEnAuto"));
        chkBoxEnAuto->setEnabled(false);

        horizontalLayout->addWidget(chkBoxEnAuto);


        verticalLayout->addLayout(horizontalLayout);

        horizontalLayout_3 = new QHBoxLayout();
        horizontalLayout_3->setSpacing(6);
        horizontalLayout_3->setObjectName(QString::fromUtf8("horizontalLayout_3"));
        label_3 = new QLabel(centralWidget);
        label_3->setObjectName(QString::fromUtf8("label_3"));

        horizontalLayout_3->addWidget(label_3);

        lineEditImageFn = new QLineEdit(centralWidget);
        lineEditImageFn->setObjectName(QString::fromUtf8("lineEditImageFn"));
        lineEditImageFn->setReadOnly(true);

        horizontalLayout_3->addWidget(lineEditImageFn);

        btnSelectFile = new QPushButton(centralWidget);
        btnSelectFile->setObjectName(QString::fromUtf8("btnSelectFile"));
        btnSelectFile->setEnabled(false);

        horizontalLayout_3->addWidget(btnSelectFile);


        verticalLayout->addLayout(horizontalLayout_3);

        MainWindow->setCentralWidget(centralWidget);
        menuBar = new QMenuBar(MainWindow);
        menuBar->setObjectName(QString::fromUtf8("menuBar"));
        menuBar->setGeometry(QRect(0, 0, 581, 25));
        MainWindow->setMenuBar(menuBar);
        mainToolBar = new QToolBar(MainWindow);
        mainToolBar->setObjectName(QString::fromUtf8("mainToolBar"));
        MainWindow->addToolBar(Qt::TopToolBarArea, mainToolBar);
        statusBar = new QStatusBar(MainWindow);
        statusBar->setObjectName(QString::fromUtf8("statusBar"));
        MainWindow->setStatusBar(statusBar);
        toolBar = new QToolBar(MainWindow);
        toolBar->setObjectName(QString::fromUtf8("toolBar"));
        MainWindow->addToolBar(Qt::TopToolBarArea, toolBar);
        toolBar_2 = new QToolBar(MainWindow);
        toolBar_2->setObjectName(QString::fromUtf8("toolBar_2"));
        MainWindow->addToolBar(Qt::TopToolBarArea, toolBar_2);

        retranslateUi(MainWindow);

        QMetaObject::connectSlotsByName(MainWindow);
    } // setupUi

    void retranslateUi(QMainWindow *MainWindow)
    {
        MainWindow->setWindowTitle(QApplication::translate("MainWindow", "MainWindow", 0, QApplication::UnicodeUTF8));
        label->setText(QApplication::translate("MainWindow", "Port:", 0, QApplication::UnicodeUTF8));
        btnBubble->setText(QApplication::translate("MainWindow", "Bubble!", 0, QApplication::UnicodeUTF8));
        label_2->setText(QApplication::translate("MainWindow", "Bubble Freq (ms)", 0, QApplication::UnicodeUTF8));
        lineEditFreq->setText(QApplication::translate("MainWindow", "1000", 0, QApplication::UnicodeUTF8));
        chkBoxEnAuto->setText(QApplication::translate("MainWindow", "Auto-Bubble", 0, QApplication::UnicodeUTF8));
        label_3->setText(QApplication::translate("MainWindow", "Image", 0, QApplication::UnicodeUTF8));
        btnSelectFile->setText(QApplication::translate("MainWindow", "Select File", 0, QApplication::UnicodeUTF8));
        toolBar->setWindowTitle(QApplication::translate("MainWindow", "toolBar", 0, QApplication::UnicodeUTF8));
        toolBar_2->setWindowTitle(QApplication::translate("MainWindow", "toolBar_2", 0, QApplication::UnicodeUTF8));
    } // retranslateUi

};

namespace Ui {
    class MainWindow: public Ui_MainWindow {};
} // namespace Ui

QT_END_NAMESPACE

#endif // UI_MAINWINDOW_H
